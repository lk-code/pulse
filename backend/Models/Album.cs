namespace Pulse.Api.Models;

public class Album
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Genre { get; set; } = string.Empty;
    public string? CoverArtPath { get; set; }

    public ICollection<AlbumArtist> AlbumArtists { get; set; } = [];
    public ICollection<Track> Tracks { get; set; } = [];
}
