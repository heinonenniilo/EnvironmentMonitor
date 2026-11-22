using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EnvironmentMonitor.Application.Services
{
    public class MqttMeasurementHandler : IMqttMeasurementHandler
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

        private readonly IMeasurementService _measurementService;
        private readonly ILogger<MqttMeasurementHandler> _logger;

        public MqttMeasurementHandler(IMeasurementService measurementService, ILogger<MqttMeasurementHandler> logger)
        {
            _measurementService = measurementService;
            _logger = logger;
        }

        public async Task HandleMeasurementMessage(string deviceIdentifier, string payload)
        {
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
            await _measurementService.AddMeasurements(measurements);
        }
    }
}
