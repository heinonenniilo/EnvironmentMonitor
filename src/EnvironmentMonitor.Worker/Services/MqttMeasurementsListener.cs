using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using System.Text.Json;

namespace EnvironmentMonitor.Worker.Services
{
    /// <summary>
    /// Subscribes to device measurement topics and stores received measurements through <see cref="IMeasurementService"/>.
    /// </summary>
    public sealed class MqttMeasurementsListener : BackgroundService
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly IMqttConnection _mqttConnection;
        private readonly MqttSettings _settings;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MqttMeasurementsListener> _logger;

        public MqttMeasurementsListener(IMqttConnection mqttConnection, MqttSettings settings, IServiceScopeFactory scopeFactory, ILogger<MqttMeasurementsListener> logger)
        {
            _mqttConnection = mqttConnection;
            _settings = settings;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var subscription = await _mqttConnection.Subscribe(_settings.MeasurementsSubscriptionTopic, HandleMessage, stoppingToken);
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task HandleMessage(string topic, string payload)
        {
            var deviceIdentifier = _settings.GetDeviceIdentifierFromMeasurementTopic(topic);
            if (deviceIdentifier == null)
            {
                _logger.LogWarning($"Received MQTT message on unexpected topic '{topic}'. Ignoring.");
                return;
            }

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
    }
}
