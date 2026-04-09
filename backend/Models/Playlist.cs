namespace Pulse.Api.Models;

public class Playlist
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<PlaylistTrack> PlaylistTracks { get; set; } = [];
}
