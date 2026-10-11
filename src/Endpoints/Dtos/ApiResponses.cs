namespace MyAppWeb.Endpoints.Dtos;

/// <summary>Successful, non-payload response.</summary>
public sealed record ApiResponse(string Status, string Message, DateTime Timestamp);

/// <summary>Failure response. Field names match the anonymous types used before the refactor.</summary>
public sealed record ApiErrorResponse(string Status, string? Error, DateTime Timestamp);
