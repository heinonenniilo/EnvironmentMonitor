using EnvironmentMonitor.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;

namespace EnvironmentMonitor.Infrastructure.Services
{
    /// <summary>
    /// Subscribes to device measurement topics and forwards received messages to <see cref="IMqttMeasurementHandler"/>.
    /// </summary>
    public sealed class MqttListenerService : BackgroundService
    {
        private static readonly TimeSpan ReconnectInterval = TimeSpan.FromSeconds(10);

        private readonly MqttClientService _mqttClientService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MqttListenerService> _logger;

        public MqttListenerService(MqttClientService mqttClientService, IServiceScopeFactory scopeFactory, ILogger<MqttListenerService> logger)
        {
            _mqttClientService = mqttClientService;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var client = _mqttClientService.Client;
            var subscriptionTopic = _mqttClientService.Settings.MeasurementsSubscriptionTopic;

            client.ConnectedAsync += async _ =>
            {
                _logger.LogInformation($"Subscribing to MQTT topic '{subscriptionTopic}'");
                await client.SubscribeAsync(_mqttClientService.BuildSubscribeOptions(subscriptionTopic), stoppingToken);
            };
            client.DisconnectedAsync += e =>
            {
                _logger.LogWarning(e.Exception, $"Disconnected from MQTT broker. Reason: {e.Reason}");
                return Task.CompletedTask;
            };
            client.ApplicationMessageReceivedAsync += HandleMessageAsync;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _mqttClientService.EnsureConnectedAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to connect to MQTT broker");
                }

                try
                {
                    await Task.Delay(ReconnectInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var deviceIdentifier = _mqttClientService.Settings.GetDeviceIdentifierFromMeasurementTopic(topic);
            if (deviceIdentifier == null)
            {
                _logger.LogWarning($"Received MQTT message on unexpected topic '{topic}'. Ignoring.");
                return;
            }

            try
            {
                var payload = e.ApplicationMessage.ConvertPayloadToString();
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IMqttMeasurementHandler>();
                await handler.HandleMeasurementMessage(deviceIdentifier, payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to handle MQTT message from topic '{topic}'");
            }
        }
    }
}
