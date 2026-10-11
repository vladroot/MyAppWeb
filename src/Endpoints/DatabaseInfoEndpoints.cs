using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using MyAppWeb.Configuration;
using MyAppWeb.Endpoints.Dtos;
using MyAppWeb.Services;

namespace MyAppWeb.Endpoints;

/// <summary>
/// Version probes for the three datastores: GET /postgres, GET /mongo, GET /sqlite.
/// </summary>
public static class DatabaseInfoEndpoints
{
    /// <summary>
    /// Kept as a plain (non-interpolated) string: interpolating the "Value" alias makes EF
    /// parameterize it as @p0, which Postgres rejects with a syntax error.
    /// </summary>
    private const string PostgresVersionQuery = "SELECT version() as \"Value\"";

    public static IEndpointRouteBuilder MapDatabaseInfoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/postgres", async (AppDbContext appDbContext, IMessageLog messageLog) =>
        {
            try
            {
                var version = await appDbContext.Database
                    .SqlQueryRaw<string>(PostgresVersionQuery)
                    .FirstOrDefaultAsync();

                var response = new DatabaseVersionResponse("postgresql", version?.ToString(), DateTime.UtcNow);
                messageLog.Add(JsonSerializer.Serialize(response, JsonDefaults.CamelCase));
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ApiErrorResponse("error", ex.InnerException?.ToString(), DateTime.UtcNow);
                messageLog.Add(JsonSerializer.Serialize(response, JsonDefaults.CamelCase));
                return Results.Json(response, statusCode: 503);
            }
        })
        .WithName("GetPostgres")
        .WithTags("Databases")
        .Produces<DatabaseVersionResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status503ServiceUnavailable);

        app.MapGet("/mongo", async (MongoClient mongoClient, IMessageLog messageLog, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("MongoProbe");
            try
            {
                var adminDb = mongoClient.GetDatabase("admin");
                var command = new BsonDocument("buildInfo", 1);
                var result = await adminDb.RunCommandAsync<BsonDocument>(command);

                foreach (var e in result.Elements)
                    logger.LogInformation("{MongoElement}", e.Value.ToString());

                string? mongoVersion = result["version"].ToString();

                var response = MongoProbeResponse.Ok(mongoVersion);
                string text = JsonSerializer.Serialize(response, JsonDefaults.CamelCaseIndented);
                messageLog.Add(text);
                return Results.Text(text, "application/json");
            }
            catch (Exception ex)
            {
                var response = MongoProbeResponse.Error(ex.Message);
                string text = JsonSerializer.Serialize(response, JsonDefaults.CamelCaseIndented);
                messageLog.Add(text);
                return Results.Text(text, "application/json");
            }
        })
        .WithName("GetMongo")
        .WithTags("Databases")
        .Produces<MongoProbeResponse>(StatusCodes.Status200OK);

        app.MapGet("/sqlite", async (DatabaseOptions options, IMessageLog messageLog) =>
        {
            if (string.IsNullOrEmpty(options.SqliteConnection))
                return Results.Text("SqliteConnnection not set");

            await using var conn = new SqliteConnection(options.SqliteConnection);
            await conn.OpenAsync();
            await using var cmd = new SqliteCommand("SELECT sqlite_version()", conn);
            var version = await cmd.ExecuteScalarAsync();

            var response = new DatabaseVersionResponse("sqlite", version?.ToString(), DateTime.UtcNow);
            messageLog.Add(JsonSerializer.Serialize(response, JsonDefaults.CamelCase));
            return Results.Ok(response);
        })
        .WithName("GetSqlite")
        .WithTags("Databases")
        .Produces<DatabaseVersionResponse>(StatusCodes.Status200OK);

        return app;
    }
}
