using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulse.Api.Data;
using Pulse.Api.Models;

namespace Pulse.Api.Endpoints;

public static class PlaylistEndpoints
{
    public static void MapPlaylistEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/playlists");

        group.MapGet("/", async (PulseDbContext db) =>
        {
            var playlists = await db.Playlists
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.CreatedAt,
                    TrackCount = p.PlaylistTracks.Count
                })
                .ToListAsync();

            return Results.Ok(playlists);
        });

        group.MapPost("/", async ([FromBody] CreatePlaylistRequest request, PulseDbContext db) =>
        {
            var playlist = new Playlist
            {
                Name = request.Name,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Playlists.Add(playlist);
            await db.SaveChangesAsync();

            return Results.Created($"/api/playlists/{playlist.Id}", new
            {
                playlist.Id,
                playlist.Name,
                playlist.CreatedAt,
                TrackCount = 0
            });
        });

        group.MapDelete("/{id:int}", async (int id, PulseDbContext db) =>
        {
            var playlist = await db.Playlists.FindAsync(id);
            if (playlist is null) return Results.NotFound();

            db.Playlists.Remove(playlist);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        group.MapGet("/{id:int}/tracks", async (int id, PulseDbContext db) =>
        {
            var exists = await db.Playlists.AnyAsync(p => p.Id == id);
            if (!exists) return Results.NotFound();

            var tracks = await db.PlaylistTracks
                .Where(pt => pt.PlaylistId == id)
                .OrderBy(pt => pt.Position)
                .Select(pt => new
                {
                    pt.Position,
                    Track = new
                    {
                        pt.Track.Id,
                        pt.Track.Title,
                        pt.Track.Artist,
                        pt.Track.Album,
                        pt.Track.AlbumArtist,
                        pt.Track.DurationSeconds,
                        pt.Track.FileType,
                        pt.Track.HasMatchingPair,
                        pt.Track.IsAvailable
                    }
                })
                .ToListAsync();

            return Results.Ok(tracks);
        });

        group.MapPost("/{id:int}/tracks", async (int id, [FromBody] AddTrackRequest request, PulseDbContext db) =>
        {
            var playlist = await db.Playlists.FindAsync(id);
            if (playlist is null) return Results.NotFound();

            var track = await db.Tracks.FindAsync(request.TrackId);
            if (track is null) return Results.NotFound();

            var alreadyAdded = await db.PlaylistTracks
                .AnyAsync(pt => pt.PlaylistId == id && pt.TrackId == request.TrackId);

            if (alreadyAdded) return Results.Conflict();

            var maxPosition = await db.PlaylistTracks
                .Where(pt => pt.PlaylistId == id)
                .MaxAsync(pt => (int?)pt.Position) ?? -1;

            db.PlaylistTracks.Add(new PlaylistTrack
            {
                PlaylistId = id,
                TrackId = request.TrackId,
                Position = maxPosition + 1
            });

            await db.SaveChangesAsync();
            return Results.Created();
        });

        group.MapDelete("/{id:int}/tracks/{trackId:int}", async (int id, int trackId, PulseDbContext db) =>
        {
            var pt = await db.PlaylistTracks.FindAsync(id, trackId);
            if (pt is null) return Results.NotFound();

            db.PlaylistTracks.Remove(pt);

            var following = await db.PlaylistTracks
                .Where(x => x.PlaylistId == id && x.Position > pt.Position)
                .ToListAsync();

            foreach (var item in following)
                item.Position--;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapPut("/{id:int}/tracks/reorder", async (int id, [FromBody] ReorderRequest request, PulseDbContext db) =>
        {
            var pt = await db.PlaylistTracks.FindAsync(id, request.TrackId);
            if (pt is null) return Results.NotFound();

            var allTracks = await db.PlaylistTracks
                .Where(x => x.PlaylistId == id)
                .OrderBy(x => x.Position)
                .ToListAsync();

            allTracks.Remove(pt);
            allTracks.Insert(Math.Clamp(request.NewPosition, 0, allTracks.Count), pt);

            for (var i = 0; i < allTracks.Count; i++)
                allTracks[i].Position = i;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}

public record CreatePlaylistRequest(string Name);
public record AddTrackRequest(int TrackId);
public record ReorderRequest(int TrackId, int NewPosition);
