using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues.Models;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EnvironmentMonitor.HubObserver.Functions
{
    public class DeviceMessageQueueProcessor
    {
        private readonly ILogger<DeviceMessageQueueProcessor> _logger;
        private readonly IDeviceService _deviceService;
        private readonly IDateService _dateService;

        public DeviceMessageQueueProcessor(ILogger<DeviceMessageQueueProcessor> logger, IDeviceService deviceService, IDateService dateService)
        {
            _logger = logger;
            _deviceService = deviceService;
            _dateService = dateService;
        }

        [Function(nameof(DeviceMessageQueueProcessor))]
        public async Task Run(
            [QueueTrigger("%DeviceMessagesQueueName%", Connection = "StorageAccountConnection")] QueueMessage queueMessage)
        {
            _logger.LogInformation("Processing device message from queue");

            try
            {
                // Access metadata
                _logger.LogInformation(
                    "Queue Message Metadata - MessageId: {MessageId}, InsertedOn: {InsertedOn}, ExpiresOn: {ExpiresOn}, DequeueCount: {DequeueCount}, NextVisibleOn: {NextVisibleOn}",
                    queueMessage.MessageId,
                    queueMessage.InsertedOn,
                    queueMessage.ExpiresOn,
                    queueMessage.DequeueCount,
                    queueMessage.NextVisibleOn);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var deviceMessage = JsonSerializer.Deserialize<DeviceQueueMessage>(queueMessage.MessageText, options);
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
                var attributes = deviceMessage.Attributes;

                var hasExecuted = false;
                switch (messageType)
                {
                    case QueuedMessages.SendDeviceAttributes:
                        await _deviceService.SendAttributesToDevice(deviceMessage.DeviceIdentifier, "Sent stored attributes to device. Triggered from storage queue.");
                        break;
                    case QueuedMessages.SetMotionControlStatus:                       
                        if (attributes?.ContainsKey(ApplicationConstants.QueuedMessageDefaultKey) == true)
                        {
                            var valueToSet = int.Parse(attributes[ApplicationConstants.QueuedMessageDefaultKey]);
                            await _deviceService.SetMotionControlStatus(deviceMessage.DeviceIdentifier, (MotionControlStatus)valueToSet);
                            hasExecuted = true;
                        }
                        break;
                    case QueuedMessages.SetMotionControlOnDelay:
                        if (attributes?.ContainsKey(ApplicationConstants.QueuedMessageDefaultKey) == true)
                        {
                            var valueToSet = long.Parse(attributes[ApplicationConstants.QueuedMessageDefaultKey]);
                            await _deviceService.SetMotionControlDelay(deviceMessage.DeviceIdentifier, valueToSet);
                            hasExecuted = true;
                        }
                        break;
                    default:
                        _logger.LogWarning("Unknown message type: {MessageTypeId}", deviceMessage.MessageTypeId);
                        break;
                }
                if (hasExecuted)
                {
                    await _deviceService.AckQueuedCommand(deviceMessage.DeviceIdentifier, queueMessage.MessageId, _dateService.CurrentTime()); 
                    _logger.LogInformation("Successfully processed device message");
                }
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
