using Microsoft.EntityFrameworkCore;

namespace MyAppWeb.Configuration;

/// <summary>
/// Startup-time tasks and middleware wiring, kept out of Program.cs.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Applies the EF Core migrations. Skip it with RUN_MIGRATIONS=false when booting
    /// without a reachable database (useful for local smoke tests and CI).
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue(DatabaseOptions.RunMigrationsKey, true))
        {
            app.Logger.LogInformation(
                "Database migrations skipped ({RunMigrationsKey} is disabled).",
                DatabaseOptions.RunMigrationsKey);
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // ef already built the model and OnModelCreating is done.
        await dbContext.Database.MigrateAsync();

        app.Logger.LogInformation("Database migrations applied.");
    }

    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
            app.MapOpenApi();

        app.UseHttpsRedirection();

        return app;
    }
}
