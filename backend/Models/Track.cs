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
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public string AlbumArtist { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Genre { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public FileType FileType { get; set; }
    public bool HasMatchingPair { get; set; }
    public string? CoverArtPath { get; set; }
    public DateTimeOffset FileLastModified { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsAvailable { get; set; }
}
