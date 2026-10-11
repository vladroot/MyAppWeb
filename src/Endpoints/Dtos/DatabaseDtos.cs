using System.Text.Json.Serialization;

namespace MyAppWeb.Endpoints.Dtos;

/// <summary>Response of the /postgres, /mongo and /sqlite version probes.</summary>
public sealed record DatabaseVersionResponse(string Database, string? Version, DateTime Timestamp);

/// <summary>
/// Response of GET /mongo. The probe answers with either a version or an error message, so a
/// single shape covers both; the unpopulated member is omitted from the JSON. This replaces the
/// two anonymous types the endpoint used to build inline.
/// </summary>
public sealed record MongoProbeResponse(string Database, string? Version, DateTime Timestamp, string Status, string? Message)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Version { get; init; } = Version;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; init; } = Message;

    public static MongoProbeResponse Ok(string? version)
        => new("mongodb", version, DateTime.UtcNow, "ok", null);

    public static MongoProbeResponse Error(string message)
        => new("mongodb", null, DateTime.UtcNow, "error", message);
}
