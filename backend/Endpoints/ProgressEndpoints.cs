using Microsoft.AspNetCore.Mvc;
using Pulse.Api.Data;
using Pulse.Api.Models;

namespace Pulse.Api.Endpoints;

public static class ProgressEndpoints
{
    public static void MapProgressEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/progress");

        group.MapGet("/{trackId:int}", async (int trackId, PulseDbContext db) =>
        {
            var progress = await db.PlaybackProgresses.FindAsync(trackId);
            if (progress is null)
                return Results.Ok(new { trackId, positionSeconds = 0.0, updatedAt = (DateTimeOffset?)null });

            return Results.Ok(new
            {
                trackId = progress.TrackId,
                positionSeconds = progress.PositionSeconds,
                updatedAt = progress.UpdatedAt
            });
        });

        group.MapPut("/{trackId:int}", async (int trackId, [FromBody] UpdateProgressRequest request, PulseDbContext db) =>
        {
            var progress = await db.PlaybackProgresses.FindAsync(trackId);

            if (progress is null)
            {
                progress = new PlaybackProgress
                {
                    TrackId = trackId,
                    PositionSeconds = request.PositionSeconds,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                db.PlaybackProgresses.Add(progress);
            }
            else
            {
                progress.PositionSeconds = request.PositionSeconds;
                progress.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}

public record UpdateProgressRequest(double PositionSeconds);
