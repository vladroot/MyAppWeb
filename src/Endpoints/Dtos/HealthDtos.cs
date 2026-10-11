using System.Text.Json.Serialization;

namespace MyAppWeb.Endpoints.Dtos;

/// <summary>
/// Response of GET /health, which the compose healthcheck polls. Only one of
/// <see cref="Message"/> / <see cref="Error"/> is ever populated; the other is omitted from
/// the JSON so the payload keeps the shape of the previous anonymous types.
/// <see cref="MissingKeys"/> names the individual connection settings that are not configured,
/// so a failure says what to fix instead of just reporting that something is missing.
/// </summary>
public sealed record HealthResponse
{
    public string Status { get; init; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; init; }

    /// <summary>
    /// Null on success (so the healthy payload is unchanged); on failure lists each missing
    /// connection setting so callers can script against the names instead of parsing text.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? MissingKeys { get; init; }

    public DateTime Timestamp { get; init; }

    public static HealthResponse Healthy()
        => new() { Status = "healthy", Message = "Ok", Timestamp = DateTime.UtcNow };

    public static HealthResponse Unhealthy(string error, IReadOnlyList<string> missingKeys)
        => new() { Status = "unhealthy", Error = error, MissingKeys = missingKeys, Timestamp = DateTime.UtcNow };
}

