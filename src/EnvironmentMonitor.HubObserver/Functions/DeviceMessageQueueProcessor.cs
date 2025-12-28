using System;
using System.Net.WebSockets;
using System.Text.Json;
using Azure.Storage.Queues.Models;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using EnvironmentMonitor.Domain.Models.GetModels;

namespace EnvironmentMonitor.HubObserver.Functions
{
    public class DeviceMessageQueueProcessor
    {
        private readonly ILogger<DeviceMessageQueueProcessor> _logger;
        private readonly IDeviceCommandService _commandService;
        private readonly IDeviceEmailService _deviceEmailService;
        private readonly IDateService _dateService;
        private readonly IUserService _userService;

        private const int MessageScheduledLimitInMinutes = 20;

        public DeviceMessageQueueProcessor(
            ILogger<DeviceMessageQueueProcessor> logger,
            IDeviceCommandService commandService,
            IDeviceEmailService deviceEmailService,
            IUserService userService,
            IDateService dateService)
        {
            _logger = logger;
            _commandService = commandService;
            _deviceEmailService = deviceEmailService;
            _dateService = dateService;
            _userService = userService;
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

                var matchingMessages = await _commandService.GetQueuedCommands( new GetQueuedCommandsModel
                {
                    DeviceIdentifiers = [deviceMessage.DeviceIdentifier],
                    MessageIds = [queueMessage.MessageId],
                    IsExecuted = false
                });

                // Skip if message's run scheduled time is too old
                if (matchingMessages.Count == 1)
                {
                    var messageToCheck = matchingMessages.First();

                    if ((_dateService.CurrentTime() - messageToCheck.Scheduled).TotalMinutes > MessageScheduledLimitInMinutes)
                    {
                        _logger.LogWarning($"Message with id {messageToCheck.MessageId} was scheduled to run at {messageToCheck.Scheduled}, now it is: {_dateService.CurrentTime()}. Limit is {MessageScheduledLimitInMinutes} min");
                        await _commandService.AckQueuedCommand(deviceMessage.DeviceIdentifier, queueMessage.MessageId, null);
                        return;
                    }
                }

                switch (messageType)
                {
                    case QueuedMessages.SendDeviceAttributes:
                        await _commandService.SendAttributesToDevice(deviceMessage.DeviceIdentifier, "Sent stored attributes to device. Triggered from storage queue.");
                        break;
                    case QueuedMessages.SetMotionControlStatus:                       
                        if (attributes?.ContainsKey(ApplicationConstants.QueuedMessageDefaultKey) == true)
                        {
                            var valueToSet = int.Parse(attributes[ApplicationConstants.QueuedMessageDefaultKey]);
                            await _commandService.SetMotionControlStatus(deviceMessage.DeviceIdentifier, (MotionControlStatus)valueToSet);
                            hasExecuted = true;
                        }
                        break;
                    case QueuedMessages.SetMotionControlOnDelay:
                        if (attributes?.ContainsKey(ApplicationConstants.QueuedMessageDefaultKey) == true)
                        {
                            var valueToSet = long.Parse(attributes[ApplicationConstants.QueuedMessageDefaultKey]);
                            await _commandService.SetMotionControlDelay(deviceMessage.DeviceIdentifier, valueToSet);
                            hasExecuted = true;
                        }
                        break;
                    case QueuedMessages.SendDeviceEmail:
                        if (attributes?.ContainsKey(ApplicationConstants.QueuedMessageDefaultKey) == true)
                        {
                            var templateTypeValue = int.Parse(attributes[ApplicationConstants.QueuedMessageDefaultKey]);
                            await _deviceEmailService.SendDeviceEmail(deviceMessage.DeviceIdentifier, (EmailTemplateTypes)templateTypeValue, attributes);
                            hasExecuted = true;
                        }
                        break;
                    case QueuedMessages.ProcessForgetUserPasswordRequest:
                        if (attributes?.ContainsKey(ApplicationConstants.QueuedMessageDefaultKey) == true)
                        {
                            var userEmail = attributes[ApplicationConstants.QueuedMessageDefaultKey];
                            _logger.LogInformation("Triggering processing of forget user password request from hub observer");
                            await _userService.ForgotPassword(new ForgotPasswordModel
                            {
                                Email = userEmail,
                                Enqueue = false
                            });
                            hasExecuted = false; // Messages not stored to DB yet.
                        }
                        break;
                    default:
                        _logger.LogWarning("Unknown message type: {MessageTypeId}", deviceMessage.MessageTypeId);
                        break;
                }
                if (hasExecuted)
                {
                    await _commandService.AckQueuedCommand(deviceMessage.DeviceIdentifier, queueMessage.MessageId, _dateService.CurrentTime()); 
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
