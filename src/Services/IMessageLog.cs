namespace MyAppWeb.Services;

/// <summary>
/// Append-only log of the responses the app has produced, served by /watch. Replaces the
/// <c>List&lt;string&gt;</c> that used to be captured by the endpoint lambdas in Program.cs.
/// </summary>
public interface IMessageLog
{
    void Add(string message);

    IReadOnlyList<string> Snapshot();
}
