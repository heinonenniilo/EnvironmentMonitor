using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Infrastructure.Services;
using MQTTnet;
using System.Text.Json;

namespace EnvironmentMonitor.Worker.Services
{
    /// <summary>
    /// Subscribes to device measurement topics and stores received measurements.
    /// </summary>
    public sealed class MqttListenerService : BackgroundService
    {
        private static readonly TimeSpan ReconnectInterval = TimeSpan.FromSeconds(10);
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

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
                var measurements = JsonSerializer.Deserialize<SaveMeasurementsDto>(payload, SerializerOptions);
                if (measurements == null)
                {
                    _logger.LogWarning($"Empty MQTT measurement payload from device '{deviceIdentifier}'. Ignoring.");
                    return;
                }

                if (string.IsNullOrEmpty(measurements.DeviceId))
                {
                    measurements.DeviceId = deviceIdentifier;
                }
                else if (!string.Equals(measurements.DeviceId, deviceIdentifier, StringComparison.Ordinal))
                {
                    _logger.LogWarning($"MQTT payload device id '{measurements.DeviceId}' does not match topic device '{deviceIdentifier}'. Message rejected.");
                    return;
                }

                var enqueuedTime = DateTime.UtcNow;
                measurements.EnqueuedUtc = enqueuedTime;
                foreach (var measurement in measurements.Measurements)
                {
                    measurement.TimestampUtc = enqueuedTime;
                }
                measurements.Source = CommunicationChannels.Mqtt;

                _logger.LogInformation($"Received {measurements.Measurements.Count} measurements through MQTT from device '{deviceIdentifier}'");
                using var scope = _scopeFactory.CreateScope();
                var measurementService = scope.ServiceProvider.GetRequiredService<IMeasurementService>();
                await measurementService.AddMeasurements(measurements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to handle MQTT message from topic '{topic}'");
            }
        }
    }
}
