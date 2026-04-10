namespace Pulse.Api.Models;

public enum FileType
{
    Audio,
    Video
}

public class Track
{
    public int Id { get; set; }
    public int LibraryId { get; set; }
    public Library Library { get; set; } = null!;

    public int AlbumId { get; set; }
    public Album Album { get; set; } = null!;

    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public int TrackNumber { get; set; }
    public int DiscNumber { get; set; }
    public int DurationSeconds { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public FileType FileType { get; set; }
    public bool HasMatchingPair { get; set; }
    public DateTimeOffset FileLastModified { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsAvailable { get; set; }
}
