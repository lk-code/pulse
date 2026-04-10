using Microsoft.EntityFrameworkCore;
using Pulse.Api.Data;

namespace Pulse.Api.Endpoints;

public static class AlbumEndpoints
{
    public static void MapAlbumEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/albums");

        group.MapGet("/", async (PulseDbContext db, string? search, int page = 1, int pageSize = 200) =>
        {
            var query = db.Albums.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(al => al.Name.Contains(search));

            var total = await query.CountAsync();
            var albums = await query
                .OrderBy(al => al.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(al => new
                {
                    al.Id,
                    al.Name,
                    al.NormalizedName,
                    al.Year,
                    al.Genre,
                    al.CoverArtPath,
                    TrackCount = al.Tracks.Count(t => t.IsAvailable),
                    PrimaryArtist = al.AlbumArtists
                        .Where(aa => aa.IsPrimary)
                        .Select(aa => new { aa.Artist.Id, aa.Artist.Name, aa.Artist.NormalizedName })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Results.Ok(new { total, page, pageSize, albums });
        });

        group.MapGet("/{id:int}", async (int id, PulseDbContext db) =>
        {
            var album = await db.Albums
                .Where(al => al.Id == id)
                .Select(al => new
                {
                    al.Id,
                    al.Name,
                    al.NormalizedName,
                    al.Year,
                    al.Genre,
                    al.CoverArtPath,
                    Artists = al.AlbumArtists
                        .OrderByDescending(aa => aa.IsPrimary)
                        .Select(aa => new { aa.Artist.Id, aa.Artist.Name, aa.Artist.NormalizedName, aa.IsPrimary })
                        .ToList(),
                    Tracks = al.Tracks
                        .Where(t => t.IsAvailable)
                        .OrderBy(t => t.DiscNumber)
                        .ThenBy(t => t.TrackNumber)
                        .ThenBy(t => t.Title)
                        .Select(t => new
                        {
                            t.Id,
                            t.Title,
                            t.NormalizedName,
                            t.TrackNumber,
                            t.DiscNumber,
                            t.DurationSeconds,
                            t.FileType,
                            t.HasMatchingPair,
                            Artist = new { t.Artist.Id, t.Artist.Name, t.Artist.NormalizedName }
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            return album is null ? Results.NotFound() : Results.Ok(album);
        });

        group.MapGet("/{id:int}/cover", async (int id, PulseDbContext db) =>
        {
            var album = await db.Albums.FindAsync(id);
            if (album is null) return Results.NotFound();

            if (album.CoverArtPath is not null && File.Exists(album.CoverArtPath))
                return Results.File(album.CoverArtPath, "image/png");

            const string placeholder = """
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" width="100" height="100">
                  <rect width="100" height="100" fill="#1e1b4b"/>
                  <circle cx="50" cy="50" r="25" fill="none" stroke="#7c3aed" stroke-width="3"/>
                  <circle cx="50" cy="50" r="8" fill="#7c3aed"/>
                  <line x1="50" y1="25" x2="50" y2="10" stroke="#7c3aed" stroke-width="3"/>
                </svg>
                """;
            return Results.Content(placeholder, "image/svg+xml");
        });
    }
}
