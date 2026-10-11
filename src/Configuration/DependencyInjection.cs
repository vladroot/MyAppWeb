using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MyAppWeb.Services;

namespace MyAppWeb.Configuration;

/// <summary>
/// The single place where the application's own services are registered, so that
/// Program.cs stays a thin startup shell.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var databases = LoadDatabaseOptions(configuration, environment.ContentRootPath);

        // Exposed as a singleton so endpoints can depend on the resolved settings.
        services.AddSingleton(databases);

        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseNpgsql(databases.PostgresConnection);
        });

        services.AddScoped<ISaveLoad, SaveLoadController>();

        services.AddSingleton<MongoClient>(_ => new MongoClient(databases.MongoConnection));
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<MongoClient>();
            return client.GetDatabase(databases.MongoDb);
        });
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddSingleton<IMessageLog, ServerMessageLog>();

        services.AddOpenApi();

        if (environment.IsDevelopment())
            services.AddHostedService<ServiceRegistrationLogger>();

        return services;
    }

    /// <summary>
    /// Reads the connection settings from configuration (environment variables are already
    /// exposed there), falling back to the .env file next to the project for local runs.
    /// </summary>
    private static DatabaseOptions LoadDatabaseOptions(IConfiguration configuration, string contentRootPath)
    {
        var envFilePath = Path.Combine(contentRootPath, ".env");

        string? Read(string key)
        {
            var value = configuration[key];
            if (string.IsNullOrWhiteSpace(value))
                value = EnvFile.GetValue(envFilePath, key);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        return new DatabaseOptions
        {
            PostgresConnection = Required(Read(DatabaseOptions.PostgresConnectionKey), DatabaseOptions.PostgresConnectionKey),
            MongoConnection = Required(Read(DatabaseOptions.MongoConnectionKey), DatabaseOptions.MongoConnectionKey),
            MongoDb = Required(Read(DatabaseOptions.MongoDbKey), DatabaseOptions.MongoDbKey),
            SqliteConnection = Required(
                Read(DatabaseOptions.SqliteConnectionKey) ?? Read(DatabaseOptions.LegacySqliteConnectionKey),
                DatabaseOptions.LegacySqliteConnectionKey)
        };

        static string Required(string? value, string key)
            => string.IsNullOrWhiteSpace(value)
                ? throw new InvalidOperationException($"{key} in compose.yaml not set!")
                : value;
    }
}
