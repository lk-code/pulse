using Microsoft.EntityFrameworkCore;
using Pulse.Api.Data;
using Pulse.Api.Models;
using TagLib;

namespace Pulse.Api.Services;

public class LibraryScannerService(
    IServiceScopeFactory scopeFactory,
    ScanStateManager scanStateManager,
    ILogger<LibraryScannerService> logger)
{
    private static readonly HashSet<string> AudioExtensions = [".mp3", ".flac", ".ogg", ".wav", ".aac", ".m4a", ".opus"];
    private static readonly HashSet<string> VideoExtensions = [".mp4", ".webm", ".mkv", ".mov"];

    public void StartScan(int libraryId)
    {
        var state = scanStateManager.Get(libraryId);
        if (state.IsScanning) return;

        _ = Task.Run(() => ScanAsync(libraryId));
    }

    private async Task ScanAsync(int libraryId)
    {
        var state = scanStateManager.Get(libraryId);
        state.IsScanning = true;
        state.ProcessedFiles = 0;
        state.TotalFiles = 0;
        state.CurrentFile = null;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PulseDbContext>();

        try
        {
            var library = await db.Libraries.FindAsync(libraryId);
            if (library is null) return;

            var allFiles = Directory.EnumerateFiles(library.RootPath, "*", SearchOption.AllDirectories)
                .Where(f =>
                {
                    var ext = Path.GetExtension(f).ToLowerInvariant();
                    return AudioExtensions.Contains(ext) || VideoExtensions.Contains(ext);
                })
                .ToList();

            state.TotalFiles = allFiles.Count;

            var existingTracks = await db.Tracks
                .Where(t => t.LibraryId == libraryId)
                .ToDictionaryAsync(t => t.FilePath);

            foreach (var track in existingTracks.Values)
                track.IsAvailable = false;

            var filesByNameWithoutExt = allFiles
                .GroupBy(f => Path.Combine(Path.GetDirectoryName(f)!, Path.GetFileNameWithoutExtension(f)))
                .ToDictionary(g => g.Key, g => g.ToList());

            var dataPath = Environment.GetEnvironmentVariable("DATA_PATH") ?? "/data";
            var coversPath = Path.Combine(dataPath, "covers");
            Directory.CreateDirectory(coversPath);

            foreach (var filePath in allFiles)
            {
                state.CurrentFile = Path.GetFileName(filePath);

                try
                {
                    var ext = Path.GetExtension(filePath).ToLowerInvariant();
                    var fileType = AudioExtensions.Contains(ext) ? FileType.Audio : FileType.Video;
                    var lastModified = new DateTimeOffset(new FileInfo(filePath).LastWriteTimeUtc, TimeSpan.Zero);

                    var nameWithoutExt = Path.Combine(
                        Path.GetDirectoryName(filePath)!,
                        Path.GetFileNameWithoutExtension(filePath));
                    var hasPair = filesByNameWithoutExt.TryGetValue(nameWithoutExt, out var siblings)
                        && siblings!.Count > 1;

                    if (existingTracks.TryGetValue(filePath, out var existing))
                    {
                        existing.IsAvailable = true;
                        existing.HasMatchingPair = hasPair;

                        if (existing.FileLastModified != lastModified)
                        {
                            UpdateTrackMetadata(existing, filePath, coversPath);
                            existing.FileLastModified = lastModified;
                            existing.UpdatedAt = DateTimeOffset.UtcNow;
                        }
                    }
                    else
                    {
                        var newTrack = new Track
                        {
                            LibraryId = libraryId,
                            FilePath = filePath,
                            FileType = fileType,
                            HasMatchingPair = hasPair,
                            FileLastModified = lastModified,
                            IsAvailable = true,
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow
                        };

                        UpdateTrackMetadata(newTrack, filePath, coversPath);
                        db.Tracks.Add(newTrack);
                        await db.SaveChangesAsync();

                        ExtractCoverArt(newTrack, filePath, coversPath);
                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to process file: {FilePath}", filePath);
                }

                state.ProcessedFiles++;
            }

            foreach (var track in existingTracks.Values.Where(t => t.IsAvailable))
            {
                ExtractCoverArtIfMissing(track, coversPath);
            }

            library.LastScannedAt = DateTimeOffset.UtcNow;
            state.LastScannedAt = library.LastScannedAt;
            state.TrackCount = await db.Tracks.CountAsync(t => t.LibraryId == libraryId && t.IsAvailable);

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Library scan failed for library {LibraryId}", libraryId);
        }
        finally
        {
            state.IsScanning = false;
            state.CurrentFile = null;
        }
    }

    private static void UpdateTrackMetadata(Track track, string filePath, string coversPath)
    {
        try
        {
            using var tagFile = TagLib.File.Create(filePath);
            var tag = tagFile.Tag;

            track.Title = tag.Title ?? Path.GetFileNameWithoutExtension(filePath);
            track.Artist = tag.FirstPerformer ?? string.Empty;
            track.Album = tag.Album ?? string.Empty;
            track.AlbumArtist = tag.FirstAlbumArtist ?? string.Empty;
            track.Year = (int)tag.Year;
            track.Genre = tag.FirstGenre ?? string.Empty;
            track.DurationSeconds = (int)tagFile.Properties.Duration.TotalSeconds;
            track.FileType = GetFileType(filePath);
        }
        catch
        {
            track.Title = Path.GetFileNameWithoutExtension(filePath);
            track.FileType = GetFileType(filePath);
        }
    }

    private static void ExtractCoverArt(Track track, string filePath, string coversPath)
    {
        try
        {
            using var tagFile = TagLib.File.Create(filePath);
            var pictures = tagFile.Tag.Pictures;

            if (pictures.Length > 0)
            {
                var coverPath = Path.Combine(coversPath, $"{track.Id}.png");
                System.IO.File.WriteAllBytes(coverPath, pictures[0].Data.Data);
                track.CoverArtPath = coverPath;
            }
        }
        catch { }
    }

    private static void ExtractCoverArtIfMissing(Track track, string coversPath)
    {
        if (track.CoverArtPath != null) return;
        ExtractCoverArt(track, track.FilePath, coversPath);
    }

    private static FileType GetFileType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return VideoExtensions.Contains(ext) ? FileType.Video : FileType.Audio;
    }
}
