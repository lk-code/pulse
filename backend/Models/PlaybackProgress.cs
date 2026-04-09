namespace Pulse.Api.Models;

public class PlaybackProgress
{
    public int TrackId { get; set; }
    public Track Track { get; set; } = null!;
    public double PositionSeconds { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
