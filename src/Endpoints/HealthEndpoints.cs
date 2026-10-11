using System.Text.Json;
using MyAppWeb.Configuration;
using MyAppWeb.Endpoints.Dtos;
using MyAppWeb.Services;

namespace MyAppWeb.Endpoints;

/// <summary>GET /health — polled by the healthcheck in compose.yaml.</summary>
public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", (DatabaseOptions options, IMessageLog messageLog) =>
        {
            string? error = options.Validate();

            if (!string.IsNullOrEmpty(error))
            {
                var response = HealthResponse.Unhealthy(error, options.MissingKeys);
                messageLog.Add(JsonSerializer.Serialize(response, JsonDefaults.CamelCase));
                return Results.Json(response, statusCode: 503);
            }

            var healthy = HealthResponse.Healthy();
            messageLog.Add(JsonSerializer.Serialize(healthy, JsonDefaults.CamelCase));
            return Results.Ok(healthy);
        })
        .WithName("GetHealth")
        .WithTags("Health")
        .Produces<HealthResponse>(StatusCodes.Status200OK)
        .Produces<HealthResponse>(StatusCodes.Status503ServiceUnavailable);

        return app;
    }
}
