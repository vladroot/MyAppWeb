using System.Collections.Concurrent;

namespace MyAppWeb.Services;

/// <summary>
/// Thread-safe, capacity-limited message log. The previous plain <c>List&lt;string&gt;</c>
/// could be corrupted by concurrent requests.
/// </summary>
public sealed class ServerMessageLog : IMessageLog
{
    public const int Capacity = 200;

    private readonly ConcurrentQueue<string> _messages = new();

    public void Add(string message)
    {
        _messages.Enqueue(message);

        while (_messages.Count > Capacity && _messages.TryDequeue(out _))
        {
            // drop the oldest entries
        }
    }

    public IReadOnlyList<string> Snapshot() => _messages.ToArray();
}
