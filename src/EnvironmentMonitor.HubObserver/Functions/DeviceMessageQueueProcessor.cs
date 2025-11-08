using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EnvironmentMonitor.HubObserver.Functions
{
    public class DeviceMessageQueueProcessor
    {
        private readonly ILogger<DeviceMessageQueueProcessor> _logger;
        private readonly IDeviceService _deviceService;

        public DeviceMessageQueueProcessor(ILogger<DeviceMessageQueueProcessor> logger, IDeviceService deviceService)
        {
            _logger = logger;
            _deviceService = deviceService;
        }

        [Function(nameof(DeviceMessageQueueProcessor))]
        public async Task Run(
            [QueueTrigger("%DeviceMessagesQueueName%", Connection = "StorageAccountConnection")] string queueMessage)
        {
            _logger.LogInformation("Processing device message from queue");

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var deviceMessage = JsonSerializer.Deserialize<DeviceQueueMessage>(queueMessage, options);
                if (deviceMessage == null)
                {
                    _logger.LogWarning("Failed to deserialize queue message");
                    return;
                }

                _logger.LogInformation(
                    "Received message - DeviceIdentifier: {DeviceIdentifier}, MessageTypeId: {MessageTypeId}",
                    deviceMessage.DeviceIdentifier,
                    deviceMessage.MessageTypeId);

                QueuedMessages messageType = (QueuedMessages)deviceMessage.MessageTypeId;
                switch (messageType)
                {
                    case QueuedMessages.SendDeviceAttributes:
                        await _deviceService.SendAttributesToDevice(deviceMessage.DeviceIdentifier);
                        break;
                    default:
                        _logger.LogWarning("Unknown message type: {MessageTypeId}", deviceMessage.MessageTypeId);
                        break;
                }
                _logger.LogInformation("Successfully processed device message");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize queue message");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing device message");
                throw;
            }

            await Task.CompletedTask;
        }
    }
}
