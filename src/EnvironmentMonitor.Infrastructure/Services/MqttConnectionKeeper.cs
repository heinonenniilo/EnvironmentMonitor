using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnvironmentMonitor.Infrastructure.Services
{
    /// <summary>
    /// Keeps the shared <see cref="IMqttConnection"/> connected, reconnecting periodically if the connection drops.
    /// </summary>
    internal sealed class MqttConnectionKeeper : BackgroundService
    {
        private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(10);

        private readonly IMqttConnection _connection;
        private readonly ILogger<MqttConnectionKeeper> _logger;

        public MqttConnectionKeeper([FromKeyedServices(MqttSettings.DefaultConnectionKey)] IMqttConnection connection, ILogger<MqttConnectionKeeper> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(CheckInterval);
            try
            {
                do
                {
                    try
                    {
                        await _connection.EnsureConnectedAsync(stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to connect to MQTT broker");
                    }
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
        }
    }
}
