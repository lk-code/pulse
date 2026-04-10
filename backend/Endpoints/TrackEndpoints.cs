using Microsoft.EntityFrameworkCore;
using Pulse.Api.Data;

namespace Pulse.Api.Endpoints;

public static class TrackEndpoints
{
    private static readonly Dictionary<string, string> MimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".mp3"] = "audio/mpeg",
        [".flac"] = "audio/flac",
        [".ogg"] = "audio/ogg",
        [".wav"] = "audio/wav",
        [".aac"] = "audio/aac",
        [".m4a"] = "audio/mp4",
        [".opus"] = "audio/opus",
        [".mp4"] = "video/mp4",
        [".webm"] = "video/webm",
        [".mkv"] = "video/x-matroska",
        [".mov"] = "video/quicktime"
    };

    public static void MapTrackEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tracks");

        group.MapGet("/", async (
            PulseDbContext db,
            int? libraryId,
            int? albumId,
            int? artistId,
            string? search,
            int page = 1,
            int pageSize = 50) =>
        {
            var query = db.Tracks.Where(t => t.IsAvailable).AsQueryable();

            if (libraryId.HasValue)
                query = query.Where(t => t.LibraryId == libraryId.Value);

            if (albumId.HasValue)
                query = query.Where(t => t.AlbumId == albumId.Value);

            if (artistId.HasValue)
                query = query.Where(t => t.ArtistId == artistId.Value ||
                                         t.Album.AlbumArtists.Any(aa => aa.ArtistId == artistId.Value));

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t =>
                    t.Title.Contains(search) ||
                    t.Artist.Name.Contains(search) ||
                    t.Album.Name.Contains(search));

            var total = await query.CountAsync();
            var tracks = await query
                .OrderBy(t => t.Album.Name)
                .ThenBy(t => t.DiscNumber)
                .ThenBy(t => t.TrackNumber)
                .ThenBy(t => t.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    t.LibraryId,
                    t.Title,
                    t.NormalizedName,
                    t.TrackNumber,
                    t.DiscNumber,
                    t.DurationSeconds,
                    t.FileType,
                    t.HasMatchingPair,
                    t.IsAvailable,
                    Artist = new { t.Artist.Id, t.Artist.Name, t.Artist.NormalizedName },
                    Album = new
                    {
                        t.Album.Id,
                        t.Album.Name,
                        t.Album.NormalizedName,
                        t.Album.Year,
                        PrimaryArtist = t.Album.AlbumArtists
                            .Where(aa => aa.IsPrimary)
                            .Select(aa => new { aa.Artist.Id, aa.Artist.Name, aa.Artist.NormalizedName })
                            .FirstOrDefault()
                    }
                })
                .ToListAsync();

            return Results.Ok(new { total, page, pageSize, tracks });
        });

        group.MapGet("/{id:int}/stream", async (int id, PulseDbContext db) =>
        {
            var track = await db.Tracks.FindAsync(id);
            if (track is null) return Results.NotFound();
            if (!File.Exists(track.FilePath)) return Results.NotFound();

            var ext = Path.GetExtension(track.FilePath).ToLowerInvariant();
            var contentType = MimeTypes.GetValueOrDefault(ext, "application/octet-stream");

            return Results.File(track.FilePath, contentType, enableRangeProcessing: true);
        });
    }
}
