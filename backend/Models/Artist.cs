namespace Pulse.Api.Models;

public class Artist
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;

    public ICollection<AlbumArtist> AlbumArtists { get; set; } = [];
    public ICollection<Track> Tracks { get; set; } = [];
}
