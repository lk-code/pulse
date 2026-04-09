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

    private const string PlaceholderSvg = """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" width="100" height="100">
          <rect width="100" height="100" fill="#1e1b4b"/>
          <circle cx="50" cy="50" r="25" fill="none" stroke="#7c3aed" stroke-width="3"/>
          <circle cx="50" cy="50" r="8" fill="#7c3aed"/>
          <line x1="50" y1="25" x2="50" y2="10" stroke="#7c3aed" stroke-width="3"/>
        </svg>
        """;

    public static void MapTrackEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tracks");

        group.MapGet("/", async (
            HttpContext ctx,
            PulseDbContext db,
            int? libraryId,
            string? search,
            string? artist,
            string? album,
            int page = 1,
            int pageSize = 50) =>
        {
            var query = db.Tracks.Where(t => t.IsAvailable).AsQueryable();

            if (libraryId.HasValue)
                query = query.Where(t => t.LibraryId == libraryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t =>
                    t.Title.Contains(search) ||
                    t.Artist.Contains(search) ||
                    t.Album.Contains(search));

            if (!string.IsNullOrWhiteSpace(artist))
                query = query.Where(t => t.Artist == artist);

            if (!string.IsNullOrWhiteSpace(album))
                query = query.Where(t => t.Album == album);

            var total = await query.CountAsync();
            var tracks = await query
                .OrderBy(t => t.AlbumArtist)
                .ThenBy(t => t.Album)
                .ThenBy(t => t.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    t.LibraryId,
                    t.Title,
                    t.Artist,
                    t.Album,
                    t.AlbumArtist,
                    t.Year,
                    t.Genre,
                    t.DurationSeconds,
                    t.FileType,
                    t.HasMatchingPair,
                    t.IsAvailable
                })
                .ToListAsync();

            return Results.Ok(new { total, page, pageSize, tracks });
        });

        group.MapGet("/{id:int}", async (int id, PulseDbContext db) =>
        {
            var track = await db.Tracks
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.LibraryId,
                    t.Title,
                    t.Artist,
                    t.Album,
                    t.AlbumArtist,
                    t.Year,
                    t.Genre,
                    t.DurationSeconds,
                    t.FileType,
                    t.HasMatchingPair,
                    t.IsAvailable
                })
                .FirstOrDefaultAsync();

            return track is null ? Results.NotFound() : Results.Ok(track);
        });

        group.MapGet("/{id:int}/stream", async (int id, HttpContext ctx, PulseDbContext db) =>
        {
            var track = await db.Tracks.FindAsync(id);
            if (track is null) return Results.NotFound();
            if (!File.Exists(track.FilePath)) return Results.NotFound();

            var ext = Path.GetExtension(track.FilePath).ToLowerInvariant();
            var contentType = MimeTypes.GetValueOrDefault(ext, "application/octet-stream");

            var fileInfo = new FileInfo(track.FilePath);
            var fileSize = fileInfo.Length;

            var rangeHeader = ctx.Request.Headers.Range.ToString();
            if (!string.IsNullOrEmpty(rangeHeader) && rangeHeader.StartsWith("bytes="))
            {
                var range = rangeHeader["bytes=".Length..];
                var parts = range.Split('-');
                var start = long.Parse(parts[0]);
                var end = parts[1].Length > 0 ? long.Parse(parts[1]) : fileSize - 1;
                end = Math.Min(end, fileSize - 1);
                var length = end - start + 1;

                ctx.Response.StatusCode = 206;
                ctx.Response.Headers.ContentType = contentType;
                ctx.Response.Headers.ContentLength = length;
                ctx.Response.Headers.Append("Content-Range", $"bytes {start}-{end}/{fileSize}");
                ctx.Response.Headers.Append("Accept-Ranges", "bytes");

                using var fs = new FileStream(track.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                fs.Seek(start, SeekOrigin.Begin);

                var buffer = new byte[81920];
                var remaining = length;

                while (remaining > 0)
                {
                    var toRead = (int)Math.Min(buffer.Length, remaining);
                    var read = await fs.ReadAsync(buffer.AsMemory(0, toRead));
                    if (read == 0) break;
                    await ctx.Response.Body.WriteAsync(buffer.AsMemory(0, read));
                    remaining -= read;
                }

                return Results.Empty;
            }

            ctx.Response.Headers.Append("Accept-Ranges", "bytes");
            return Results.File(track.FilePath, contentType, enableRangeProcessing: true);
        });

        group.MapGet("/{id:int}/cover", async (int id, PulseDbContext db) =>
        {
            var track = await db.Tracks.FindAsync(id);
            if (track is null) return Results.NotFound();

            if (track.CoverArtPath is not null && File.Exists(track.CoverArtPath))
                return Results.File(track.CoverArtPath, "image/png");

            return Results.Content(PlaceholderSvg, "image/svg+xml");
        });
    }
}
