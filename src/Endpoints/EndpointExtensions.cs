namespace MyAppWeb.Endpoints;

/// <summary>
/// Single entry point that wires up every endpoint group, so Program.cs only needs one call.
/// Route templates and .WithName() values are unchanged, which keeps the compose healthcheck
/// (GET /health) and any existing clients working.
/// </summary>
public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapWatchEndpoints();
        app.MapProductEndpoints();
        app.MapHealthEndpoints();
        app.MapDatabaseInfoEndpoints();

        return app;
    }
}
