using Microsoft.EntityFrameworkCore;
using Pulse.Api.Data;
using Pulse.Api.Models;

namespace Pulse.Api.Services;

public class LibraryScannerService(
    IServiceScopeFactory scopeFactory,
    ScanStateManager scanStateManager,
    ILogger<LibraryScannerService> logger,
    IConfiguration configuration)
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

            var enumerationOptions = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true,
                ReturnSpecialDirectories = false,
                AttributesToSkip = FileAttributes.System | FileAttributes.ReparsePoint
            };

            var allFiles = Directory.EnumerateFiles(library.RootPath, "*", enumerationOptions)
                .Where(f =>
                {
                    var ext = Path.GetExtension(f).ToLowerInvariant();
                    return AudioExtensions.Contains(ext) || VideoExtensions.Contains(ext);
                })
                .ToList();

            state.TotalFiles = allFiles.Count;

            // Mark all existing tracks for this library as unavailable
            var existingTracks = await db.Tracks
                .Where(t => t.LibraryId == libraryId)
                .ToDictionaryAsync(t => t.FilePath);

            foreach (var track in existingTracks.Values)
                track.IsAvailable = false;

            await db.SaveChangesAsync();

            // Detect paired files (same filename, different extension)
            var filesByNameWithoutExt = allFiles
                .GroupBy(f => Path.Combine(Path.GetDirectoryName(f)!, Path.GetFileNameWithoutExtension(f)))
                .ToDictionary(g => g.Key, g => g.ToList());

            var dataPath = configuration["Pulse:DataPath"] ?? "/data";
            var coversPath = Path.Combine(dataPath, "covers");
            Directory.CreateDirectory(coversPath);

            // In-memory caches for this scan to avoid repeated DB lookups
            var artistCache = new Dictionary<string, Artist>(StringComparer.Ordinal);
            var albumCache = new Dictionary<string, Album>(StringComparer.Ordinal);

            // Seed caches with already existing entities
            await foreach (var a in db.Artists.AsAsyncEnumerable())
                artistCache[a.NormalizedName] = a;

            await foreach (var al in db.Albums.Include(a => a.AlbumArtists).AsAsyncEnumerable())
            {
                var primary = al.AlbumArtists.FirstOrDefault(aa => aa.IsPrimary);
                if (primary is not null)
                    albumCache[$"{primary.ArtistId}:{al.NormalizedName}"] = al;
            }

            foreach (var filePath in allFiles)
            {
                state.CurrentFile = Path.GetFileName(filePath);

                try
                {
                    var nameWithoutExt = Path.Combine(
                        Path.GetDirectoryName(filePath)!,
                        Path.GetFileNameWithoutExtension(filePath));
                    var hasPair = filesByNameWithoutExt.TryGetValue(nameWithoutExt, out var siblings)
                        && siblings!.Count > 1;

                    var lastModified = new DateTimeOffset(new FileInfo(filePath).LastWriteTimeUtc, TimeSpan.Zero);
                    var ext = Path.GetExtension(filePath).ToLowerInvariant();
                    var fileType = AudioExtensions.Contains(ext) ? FileType.Audio : FileType.Video;

                    var meta = ReadMetadata(filePath);

                    // Resolve or create the primary (album) artist
                    var albumArtist = await GetOrCreateArtistAsync(db, artistCache, meta.AlbumArtistName);

                    // Resolve or create the track artist (may differ from album artist)
                    var trackArtist = meta.TrackArtistName == meta.AlbumArtistName
                        ? albumArtist
                        : await GetOrCreateArtistAsync(db, artistCache, meta.TrackArtistName);

                    // Resolve or create the album
                    var album = await GetOrCreateAlbumAsync(db, albumCache, albumArtist, trackArtist, meta, coversPath);

                    if (existingTracks.TryGetValue(filePath, out var existing))
                    {
                        existing.IsAvailable = true;
                        existing.HasMatchingPair = hasPair;
                        existing.ArtistId = trackArtist.Id;
                        existing.AlbumId = album.Id;

                        if (existing.FileLastModified != lastModified)
                        {
                            existing.Title = meta.Title;
                            existing.NormalizedName = Normalizer.Normalize(meta.Title);
                            existing.TrackNumber = meta.TrackNumber;
                            existing.DiscNumber = meta.DiscNumber;
                            existing.DurationSeconds = meta.DurationSeconds;
                            existing.FileType = fileType;
                            existing.FileLastModified = lastModified;
                            existing.UpdatedAt = DateTimeOffset.UtcNow;
                        }

                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        var newTrack = new Track
                        {
                            LibraryId = libraryId,
                            AlbumId = album.Id,
                            ArtistId = trackArtist.Id,
                            FilePath = filePath,
                            FileType = fileType,
                            Title = meta.Title,
                            NormalizedName = Normalizer.Normalize(meta.Title),
                            TrackNumber = meta.TrackNumber,
                            DiscNumber = meta.DiscNumber,
                            DurationSeconds = meta.DurationSeconds,
                            HasMatchingPair = hasPair,
                            FileLastModified = lastModified,
                            IsAvailable = true,
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow
                        };

                        db.Tracks.Add(newTrack);
                        await db.SaveChangesAsync();

                        // Extract cover art for the album if not yet done
                        if (album.CoverArtPath is null)
                        {
                            ExtractCoverArt(album, filePath, coversPath);
                            await db.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to process file: {FilePath}", filePath);
                }

                state.ProcessedFiles++;
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

    private static async Task<Artist> GetOrCreateArtistAsync(
        PulseDbContext db,
        Dictionary<string, Artist> cache,
        string name)
    {
        var normalized = Normalizer.Normalize(name);
        if (cache.TryGetValue(normalized, out var existing))
            return existing;

        var artist = new Artist { Name = name, NormalizedName = normalized };
        db.Artists.Add(artist);
        await db.SaveChangesAsync();
        cache[normalized] = artist;
        return artist;
    }

    private static async Task<Album> GetOrCreateAlbumAsync(
        PulseDbContext db,
        Dictionary<string, Album> albumCache,
        Artist primaryArtist,
        Artist trackArtist,
        TrackMetadata meta,
        string coversPath)
    {
        var normalizedAlbum = Normalizer.Normalize(meta.AlbumName);
        var cacheKey = $"{primaryArtist.Id}:{normalizedAlbum}";

        if (!albumCache.TryGetValue(cacheKey, out var album))
        {
            album = new Album
            {
                Name = meta.AlbumName,
                NormalizedName = normalizedAlbum,
                Year = meta.Year,
                Genre = meta.Genre
            };
            db.Albums.Add(album);
            await db.SaveChangesAsync();

            db.AlbumArtists.Add(new AlbumArtist { AlbumId = album.Id, ArtistId = primaryArtist.Id, IsPrimary = true });
            await db.SaveChangesAsync();

            // Reload with AlbumArtists so the cache entry is complete
            album.AlbumArtists = [new AlbumArtist { AlbumId = album.Id, ArtistId = primaryArtist.Id, IsPrimary = true }];
            albumCache[cacheKey] = album;
        }

        // Link track artist to album if different from primary and not yet linked
        if (trackArtist.Id != primaryArtist.Id)
        {
            var alreadyLinked = album.AlbumArtists.Any(aa => aa.ArtistId == trackArtist.Id);
            if (!alreadyLinked)
            {
                var exists = await db.AlbumArtists.AnyAsync(aa => aa.AlbumId == album.Id && aa.ArtistId == trackArtist.Id);
                if (!exists)
                {
                    db.AlbumArtists.Add(new AlbumArtist { AlbumId = album.Id, ArtistId = trackArtist.Id, IsPrimary = false });
                    await db.SaveChangesAsync();
                    album.AlbumArtists.Add(new AlbumArtist { AlbumId = album.Id, ArtistId = trackArtist.Id, IsPrimary = false });
                }
            }
        }

        return album;
    }

    private static void ExtractCoverArt(Album album, string filePath, string coversPath)
    {
        try
        {
            using var tagFile = TagLib.File.Create(filePath);
            var pictures = tagFile.Tag.Pictures;
            if (pictures.Length > 0)
            {
                var coverPath = Path.Combine(coversPath, $"album-{album.Id}.png");
                File.WriteAllBytes(coverPath, pictures[0].Data.Data);
                album.CoverArtPath = coverPath;
            }
        }
        catch { }
    }

    private static TrackMetadata ReadMetadata(string filePath)
    {
        try
        {
            using var tagFile = TagLib.File.Create(filePath);
            var tag = tagFile.Tag;

            var albumArtistName = tag.FirstAlbumArtist?.Trim();
            var trackArtistName = tag.FirstPerformer?.Trim();
            var primary = !string.IsNullOrEmpty(albumArtistName) ? albumArtistName
                        : !string.IsNullOrEmpty(trackArtistName) ? trackArtistName
                        : "Unknown Artist";
            var track = !string.IsNullOrEmpty(trackArtistName) ? trackArtistName : primary;

            return new TrackMetadata
            {
                Title = !string.IsNullOrEmpty(tag.Title) ? tag.Title.Trim() : Path.GetFileNameWithoutExtension(filePath),
                AlbumArtistName = primary,
                TrackArtistName = track,
                AlbumName = !string.IsNullOrEmpty(tag.Album) ? tag.Album.Trim() : "Unknown Album",
                Year = (int)tag.Year,
                Genre = tag.FirstGenre?.Trim() ?? string.Empty,
                TrackNumber = (int)tag.Track,
                DiscNumber = (int)(tag.Disc > 0 ? tag.Disc : 1),
                DurationSeconds = (int)tagFile.Properties.Duration.TotalSeconds
            };
        }
        catch
        {
            return new TrackMetadata
            {
                Title = Path.GetFileNameWithoutExtension(filePath),
                AlbumArtistName = "Unknown Artist",
                TrackArtistName = "Unknown Artist",
                AlbumName = "Unknown Album"
            };
        }
    }

    private sealed record TrackMetadata
    {
        public string Title { get; init; } = string.Empty;
        public string AlbumArtistName { get; init; } = string.Empty;
        public string TrackArtistName { get; init; } = string.Empty;
        public string AlbumName { get; init; } = string.Empty;
        public int Year { get; init; }
        public string Genre { get; init; } = string.Empty;
        public int TrackNumber { get; init; }
        public int DiscNumber { get; init; }
        public int DurationSeconds { get; init; }
    }

    private static FileType GetFileType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return VideoExtensions.Contains(ext) ? FileType.Video : FileType.Audio;
    }
}
