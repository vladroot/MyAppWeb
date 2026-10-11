namespace MyAppWeb.Configuration;

/// <summary>
/// Minimal .env reader (KEY=VALUE lines, '#' comments, optional quotes). Shared by the
/// runtime and DesignTimeDbContextFactory so the format is parsed in exactly one place.
/// </summary>
internal static class EnvFile
{
    public static string? GetValue(string envFilePath, string key)
    {
        if (!File.Exists(envFilePath))
            return null;

        foreach (var line in File.ReadAllLines(envFilePath))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                continue;

            var separatorIndex = trimmed.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            if (trimmed[..separatorIndex].Trim() != key)
                continue;

            var value = trimmed[(separatorIndex + 1)..].Trim().Trim('"');
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        return null;
    }
}
