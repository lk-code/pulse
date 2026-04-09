using System.Collections.Concurrent;

namespace Pulse.Api.Services;

public class ScanStateManager
{
    private readonly ConcurrentDictionary<int, ScanState> _states = new();

    public ScanState GetOrCreate(int libraryId)
        => _states.GetOrAdd(libraryId, _ => new ScanState());

    public ScanState Get(int libraryId)
        => _states.GetOrAdd(libraryId, _ => new ScanState());
}
