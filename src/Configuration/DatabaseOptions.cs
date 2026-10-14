namespace MyAppWeb.Configuration;

/// <summary>
/// Connection settings for the three datastores the app talks to. Values arrive as
/// top-level environment variables injected by compose.yaml.
/// </summary>
public class DatabaseOptions
{
    public const string PostgresConnectionKey = "PostgresConnection";
    public const string MongoConnectionKey = "MongoConnection";
    public const string MongoDbKey = "MongoDb";
    public const string SqliteConnectionKey = "SqliteConnection";

    /// <summary>
    /// Optional fourth datastore. When it is empty the app falls back to an in-memory
    /// <c>IDistributedCache</c>, so the endpoints keep working without a Redis instance.
    /// </summary>
    public const string RedisConnectionKey = "RedisConnection";

    /// <summary>Key prefix shared by every cached entry (set as the Redis instance name).</summary>
    public const string RedisInstanceName = "MyAppWeb:";

    /// <summary>
    /// compose.yaml ships this key misspelled with three n's; we accept both spellings.
    /// </summary>
    public const string LegacySqliteConnectionKey = "SqliteConnnection";

    /// <summary>Set to "false" to boot the app without applying EF Core migrations.</summary>
    public const string RunMigrationsKey = "RUN_MIGRATIONS";

    /// <summary>Suffix appended to each missing key, matching the original wording.</summary>
    public const string NotConfiguredSuffix = " in compose.yaml not set";

    public string PostgresConnection { get; init; } = string.Empty;
    public string MongoConnection { get; init; } = string.Empty;
    public string MongoDb { get; init; } = string.Empty;
    public string SqliteConnection { get; init; } = string.Empty;

    /// <summary>
    /// StackExchange.Redis configuration, e.g. <c>redis:6379,password=...,abortConnect=false</c>.
    /// Deliberately not part of <see cref="Validate"/>/<see cref="MissingKeys"/>: Redis is a
    /// cache, not a system of record, so a missing entry must not make /health report unhealthy.
    /// </summary>
    public string RedisConnection { get; init; } = string.Empty;

    /// <summary>True when caching is backed by Redis instead of the in-memory fallback.</summary>
    public bool IsRedisConfigured => !string.IsNullOrWhiteSpace(RedisConnection);

    /// <summary>The names of every connection setting that is not configured.</summary>
    public IReadOnlyList<string> MissingKeys =>
        MissingEntries().Select(entry => entry.Key).ToList();

    /// <summary>
    /// Returns a message naming every unconfigured connection setting, or <c>null</c> when
    /// the configuration is complete. Naming each key makes /health say what to fix instead
    /// of just reporting that something is missing.
    /// </summary>
    public string? Validate()
    {
        var missing = MissingEntries().Select(entry => entry.Message).ToList();
        return missing.Count == 0 ? null : string.Join("; ", missing);
    }

    /// <summary>
    /// Reports the missing settings using the key compose.yaml actually defines, so the
    /// misspelled SqliteConnnection is the name operators have to fix.
    /// </summary>
    private IEnumerable<(string Key, string Message)> MissingEntries()
    {
        if (string.IsNullOrWhiteSpace(PostgresConnection))
            yield return (PostgresConnectionKey, PostgresConnectionKey + NotConfiguredSuffix);

        if (string.IsNullOrWhiteSpace(MongoConnection))
            yield return (MongoConnectionKey, MongoConnectionKey + NotConfiguredSuffix);

        if (string.IsNullOrWhiteSpace(MongoDb))
            yield return (MongoDbKey, MongoDbKey + NotConfiguredSuffix);

        if (string.IsNullOrWhiteSpace(SqliteConnection))
            yield return (LegacySqliteConnectionKey, LegacySqliteConnectionKey + NotConfiguredSuffix);
    }
}

