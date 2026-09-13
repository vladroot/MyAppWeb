using System.Text.Json;
using MyAppWeb;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.Bson;

var builder = WebApplication.CreateBuilder(args);

// Postgres
var postgresqlConnection = Environment.GetEnvironmentVariable("PostgresConnection")
    ?? throw new Exception("PostgresConnection in compose.yaml not set!");
var mongoConnectionString = Environment.GetEnvironmentVariable("MongoConnection")
    ?? throw new Exception("MongoConnection in compose.yaml not set!");
var mongoDbString = Environment.GetEnvironmentVariable("MongoDb")
    ?? throw new Exception("MongoDb in compose.yaml not set!");
var sqliteConnection = Environment.GetEnvironmentVariable("SqliteConnnection")
    ?? throw new Exception("SqliteConnnection in compose.yaml not set!");

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(postgresqlConnection));

builder.Services.AddScoped<IAppDbContext, AppDbContext>();
builder.Services.AddScoped<ISaveLoad, SaveLoadController>();

builder.Services.AddSingleton<MongoClient>(sp => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<MongoClient>();
    return client.GetDatabase(mongoDbString);
});
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddOpenApi();

List<string> srvMessages = new List<string>();

var services = builder.Services.OrderBy(o => o.Lifetime);
int i = 1;
foreach (var service in services)
{
    Console.WriteLine($"{i} \t {service.Lifetime} \t {service.ServiceType.FullName}");
    i++;
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/watch", () =>
{
    if (srvMessages.Count == 0)
        return ["Nothing here yet"];
    return srvMessages;
})
.WithName("GetWeatherForecast");

app.MapGet("/json", (ISaveLoad saveLoad) =>
{
    string json = saveLoad.DoWork();
    srvMessages.Add(json);
    return json;
}).WithName("GetJson");

app.MapGet("/health", async context =>
{
    string text;
    if (string.IsNullOrEmpty(postgresqlConnection) ||
        string.IsNullOrEmpty(sqliteConnection) ||
        string.IsNullOrEmpty(mongoConnectionString) ||
        string.IsNullOrEmpty(mongoDbString))
    {
        var response = new
        {
            status = "unhealthy",
            error = "PostgresConnection in compose.yaml not set",
            timestamp = DateTime.UtcNow
        };

        context.Response.StatusCode = 503;
        context.Response.ContentType = "application/json";
        text = JsonSerializer.Serialize(response);
    }
    else
    {
        var response = new
        {
            status = "healthy",
            message = "Ok",
            timestamp = DateTime.UtcNow
        };
        context.Response.ContentType = "application/json";
        text = JsonSerializer.Serialize(response);
    }

    srvMessages.Add(text);
    await context.Response.WriteAsync(text);
});

app.MapGet("/postgres", async (HttpContext context, AppDbContext appDbContext) =>
{
    string text = string.Empty;
    try
    {
        var authHeader = context.Request.Headers.Authorization.ToString();
        var version = await appDbContext.Database.SqlQuery<string>($"SELECT version() as \"Value\"").FirstOrDefaultAsync();

        // return Results.Ok(new { version, auth = authHeader });
        var response = new
        {
            database = "postgresql",
            version = version?.ToString(),
            timestamp = DateTime.UtcNow
        };

        context.Response.ContentType = "application/json";
        text = JsonSerializer.Serialize(response);
    }
    catch (Exception ex)
    {
        var response = new
        {
            status = "error",
            error = ex.InnerException?.ToString(),
            timestamp = DateTime.UtcNow
        };

        context.Response.StatusCode = 503;
        context.Response.ContentType = "application/json";
        text = JsonSerializer.Serialize(response);
    }
    srvMessages.Add(text);
    await context.Response.WriteAsync(text);
});

app.MapGet("/mongo", async (HttpContext context, MongoClient mongoClient) =>
{
    string text = string.Empty;
    try
    {
        var adminDb = mongoClient.GetDatabase("admin");
        var command = new BsonDocument("buildInfo", 1);
        var result = await adminDb.RunCommandAsync<BsonDocument>(command);

        foreach (var e in result.Elements)
            Console.WriteLine(e.Value.ToString());

        string? mongoVersion = result["version"].ToString();

        var response = new
        {
            database = "mongodb",
            version = mongoVersion,
            timestamp = DateTime.UtcNow,
            status = "ok"
        };

        context.Response.ContentType = "application/json";
        text = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
    }
    catch (Exception ex)
    {
        var errorResponse = new
        {
            database = "mongodb",
            timestamp = DateTime.UtcNow,
            status = "error",
            message = ex.Message
        };
        context.Response.ContentType = "application/json";
        text = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { WriteIndented = true });
    }

    srvMessages.Add(text);
    await context.Response.WriteAsync(text);
});

app.MapGet("/sqlite", async context =>
{
    string text;
    if (string.IsNullOrEmpty(sqliteConnection))
    {
        text = "SqliteConnnection not set";
    }
    else
    {

        await using var conn = new SqliteConnection(sqliteConnection);
        await conn.OpenAsync();
        await using var cmd = new SqliteCommand("SELECT sqlite_version()", conn);
        var version = await cmd.ExecuteScalarAsync();

        var response = new { database = "sqlite", version = version?.ToString(), timestamp = DateTime.UtcNow };
        context.Response.ContentType = "application/json";
        text = JsonSerializer.Serialize(response);
    }
    srvMessages.Add(text);
    await context.Response.WriteAsync(text);
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
