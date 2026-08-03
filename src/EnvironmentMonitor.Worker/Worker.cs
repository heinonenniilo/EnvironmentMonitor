namespace EnvironmentMonitor.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EnvironmentMonitor Worker started at: {time}", DateTimeOffset.UtcNow);
        _logger.LogInformation("Hangfire server is processing background jobs");

        try
        {
            // Keep the service alive - Hangfire handles all job processing
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // Expected when service is stopping
            _logger.LogInformation("EnvironmentMonitor Worker stopping at: {time}", DateTimeOffset.UtcNow);
        }
    }
}
