using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pulse.Api.Data;
using Pulse.Api.Models;
using Pulse.Api.Services;

namespace Pulse.Api.Endpoints;

public static class LibraryEndpoints
{
    public static void MapLibraryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/libraries");

        group.MapGet("/", async (PulseDbContext db) =>
        {
            var libraries = await db.Libraries
                .Select(l => new
                {
                    l.Id,
                    l.Name,
                    l.RootPath,
                    l.CreatedAt,
                    l.LastScannedAt
                })
                .ToListAsync();

            return Results.Ok(libraries);
        });

        group.MapPost("/", async (
            [FromBody] CreateLibraryRequest request,
            PulseDbContext db) =>
        {
            var library = new Library
            {
                Name = request.Name,
                RootPath = request.RootPath,
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Libraries.Add(library);
            await db.SaveChangesAsync();

            return Results.Created($"/api/libraries/{library.Id}", new
            {
                library.Id,
                library.Name,
                library.RootPath,
                library.CreatedAt,
                library.LastScannedAt
            });
        });

        group.MapDelete("/{id:int}", async (int id, PulseDbContext db) =>
        {
            var library = await db.Libraries.FindAsync(id);
            if (library is null) return Results.NotFound();

            db.Libraries.Remove(library);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        group.MapPost("/{id:int}/scan", async (
            int id,
            PulseDbContext db,
            LibraryScannerService scanner) =>
        {
            var library = await db.Libraries.FindAsync(id);
            if (library is null) return Results.NotFound();

            scanner.StartScan(id);

            return Results.Accepted();
        });

        group.MapGet("/{id:int}/scan/status", async (
            int id,
            PulseDbContext db,
            ScanStateManager scanStateManager) =>
        {
            var library = await db.Libraries.FindAsync(id);
            if (library is null) return Results.NotFound();

            var state = scanStateManager.Get(id);
            var trackCount = state.TrackCount > 0
                ? state.TrackCount
                : await db.Tracks.CountAsync(t => t.LibraryId == id && t.IsAvailable);

            return Results.Ok(new
            {
                state.IsScanning,
                LastScannedAt = state.LastScannedAt ?? library.LastScannedAt,
                TrackCount = trackCount,
                state.CurrentFile,
                state.ProcessedFiles,
                state.TotalFiles
            });
        });
    }
}

public record CreateLibraryRequest(string Name, string RootPath);
