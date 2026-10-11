using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MyAppWeb.Configuration;

namespace MyAppWeb;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string PostgresConnectionKey = "POSTGRES_CONNECTION";

    public AppDbContext CreateDbContext(string[] args)
    {
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");

        if (!File.Exists(envPath))
            throw new FileNotFoundException(
                ".env not found. Ma sure file exists in project root folder.", envPath);

        string? connectionString = EnvFile.GetValue(envPath, PostgresConnectionKey);

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"{PostgresConnectionKey} not found in .env!");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
