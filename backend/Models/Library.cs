namespace Pulse.Api.Models;

public class Library
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RootPath { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastScannedAt { get; set; }
    public ICollection<Track> Tracks { get; set; } = [];
}
