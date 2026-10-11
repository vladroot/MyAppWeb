using System.Text.Json;

namespace MyAppWeb.Endpoints;

/// <summary>
/// Shared JSON settings. The framework serializes HTTP bodies with camelCase, so the copies
/// pushed into <see cref="Services.IMessageLog"/> must use the same policy — otherwise /watch
/// shows PascalCase entries while the endpoints themselves return camelCase.
/// </summary>
internal static class JsonDefaults
{
    public static readonly JsonSerializerOptions CamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static readonly JsonSerializerOptions CamelCaseIndented = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}
