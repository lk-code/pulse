namespace Pulse.Api.Services;

public class ScanState
{
    public bool IsScanning { get; set; }
    public string? CurrentFile { get; set; }
    public int ProcessedFiles { get; set; }
    public int TotalFiles { get; set; }
    public DateTimeOffset? LastScannedAt { get; set; }
    public int TrackCount { get; set; }
}
