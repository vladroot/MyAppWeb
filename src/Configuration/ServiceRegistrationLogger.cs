namespace MyAppWeb.Configuration;

/// <summary>
/// Development-only dump of the DI registrations, previously done with Console.WriteLine
/// straight inside Program.cs.
/// </summary>
internal sealed class ServiceRegistrationLogger : IHostedService
{
    private readonly IEnumerable<ServiceDescriptor> _descriptors;
    private readonly ILogger<ServiceRegistrationLogger> _logger;

    public ServiceRegistrationLogger(
        IEnumerable<ServiceDescriptor> descriptors,
        ILogger<ServiceRegistrationLogger> logger)
    {
        _descriptors = descriptors;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var index = 1;
        foreach (var service in _descriptors.OrderBy(d => d.Lifetime))
        {
            _logger.LogInformation(
                "{Index} \t {Lifetime} \t {ServiceType}",
                index++,
                service.Lifetime,
                service.ServiceType.FullName);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
