using Microsoft.EntityFrameworkCore;
using Pulse.Api.Data;

namespace Pulse.Api.Endpoints;

public static class ArtistEndpoints
{
    public static void MapArtistEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/artists");

        group.MapGet("/", async (PulseDbContext db, string? search, int page = 1, int pageSize = 200) =>
        {
            var query = db.Artists.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(a => a.Name.Contains(search));

            var total = await query.CountAsync();
            var artists = await query
                .OrderBy(a => a.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.NormalizedName,
                    AlbumCount = a.AlbumArtists.Count(aa => aa.IsPrimary),
                    TrackCount = a.Tracks.Count(t => t.IsAvailable)
                })
                .ToListAsync();

            return Results.Ok(new { total, page, pageSize, artists });
        });

        group.MapGet("/{id:int}", async (int id, PulseDbContext db) =>
        {
            var artist = await db.Artists
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.NormalizedName,
                    Albums = a.AlbumArtists
                        .Where(aa => aa.IsPrimary)
                        .Select(aa => new
                        {
                            aa.Album.Id,
                            aa.Album.Name,
                            aa.Album.NormalizedName,
                            aa.Album.Year,
                            aa.Album.Genre,
                            aa.Album.CoverArtPath,
                            TrackCount = aa.Album.Tracks.Count(t => t.IsAvailable)
                        })
                        .OrderBy(al => al.Year)
                        .ThenBy(al => al.Name)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            return artist is null ? Results.NotFound() : Results.Ok(artist);
        });
    }
}
