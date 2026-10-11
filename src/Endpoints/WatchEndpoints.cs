using MyAppWeb.Services;

namespace MyAppWeb.Endpoints;

/// <summary>GET /watch and GET /json.</summary>
public static class WatchEndpoints
{
    public static IEndpointRouteBuilder MapWatchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/watch", (IMessageLog messageLog) =>
        {
            var messages = messageLog.Snapshot();
            return messages.Count == 0
                ? Results.Ok(new[] { "Nothing here yet" })
                : Results.Ok(messages);
        })
        .WithName("GetWatch")
        .WithTags("Watch")
        .Produces<IReadOnlyList<string>>(StatusCodes.Status200OK);

        app.MapGet("/json", (ISaveLoad saveLoad, IMessageLog messageLog) =>
        {
            string json = saveLoad.DoWork();
            messageLog.Add(json);
            return Results.Text(json, "application/json");
        })
        .WithName("GetJson")
        .WithTags("Watch");

        return app;
    }
}
