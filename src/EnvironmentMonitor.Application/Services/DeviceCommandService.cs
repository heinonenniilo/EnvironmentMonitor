using AutoMapper;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Models.GetModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Services
{
    public class DeviceCommandService : IDeviceCommandService
    {
        private readonly ILogger<DeviceCommandService> _logger;
        private readonly IUserService _userService;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IMapper _mapper;
        private readonly IDateService _dateService;
        private readonly IQueueClient _queueClient;
        private readonly IHubMessageService _messageService;

        public DeviceCommandService(
            ILogger<DeviceCommandService> logger,
            IUserService userService,
            IDeviceRepository deviceRepository,
            IMapper mapper,
            IDateService dateService,
            IQueueClient queueClient,
            IHubMessageService messageService)
        {
            _logger = logger;
            _userService = userService;
            _deviceRepository = deviceRepository;
            _mapper = mapper;
            _dateService = dateService;
            _queueClient = queueClient;
            _messageService = messageService;
        }

        public async Task<List<DeviceQueuedCommandDto>> GetQueuedCommands(Guid deviceIdentifier) => await GetQueuedCommands(new GetQueuedCommandsModel()
        {
            DeviceIdentifiers = [deviceIdentifier]
        });

        public async Task<List<DeviceQueuedCommandDto>> GetQueuedCommands(GetQueuedCommandsModel model)
        {
            _logger.LogInformation($"Fetching queued commands. Device Identifiers: {string.Join(",", model.DeviceIdentifiers ?? [])}");

            if (model.DeviceIdentifiers?.Any() == true)
            {
                if (!_userService.HasAccessToDevices(model.DeviceIdentifiers, AccessLevels.Write))
                {
                    _logger.LogWarning($"No access to devices: {string.Join(",", model.DeviceIdentifiers)}");
                    throw new UnauthorizedAccessException();
                }
            }
            else if (!_userService.IsAdmin)
            {
                // If no specific devices requested and not admin, filter by user's devices
                var deviceIds = _userService.GetDevices();
                _logger.LogInformation($"User has access to: {deviceIds.Count} devices");
                if (deviceIds.Count == 0)
                {
                    throw new UnauthorizedAccessException();
                }
                model.DeviceIdentifiers = deviceIds;
            }

            var commands = await _deviceRepository.GetQueuedCommands(model);
            return _mapper.Map<List<DeviceQueuedCommandDto>>(commands);
        }

        public async Task Reboot(Guid identifier)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [identifier] })).FirstOrDefault() ?? throw new EntityNotFoundException($"Device with identifier: '{identifier}' not found.");
            if (device.IsVirtual)
            {
                throw new InvalidOperationException($"Cannot reboot a virtual device ({device.Id})");
            }
            _logger.LogInformation($"Trying to reboot device with identifier '{identifier}'");
            await _messageService.SendMessageToDevice(device.DeviceIdentifier, "REBOOT");
            await _deviceRepository.AddEvent(device.Id, DeviceEventTypes.RebootCommand, "Rebooted by UI", true, null);
        }

        public async Task<List<DeviceAttributeDto>> SetMotionControlStatus(Guid identifier, MotionControlStatus status, DateTime? triggeringTime = null)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [identifier] })).FirstOrDefault() ?? throw new EntityNotFoundException($"Device with identifier: '{identifier}' not found.");

            if (device.IsVirtual)
            {
                throw new InvalidOperationException($"Cannot send messages to a virtual device ({device.Id})");
            }

            if (triggeringTime != null)
            {
                ValidateTriggeringTime(triggeringTime.Value);
                var delay = triggeringTime.Value - _dateService.CurrentTime();
                _logger.LogInformation($"Setting 'SetMotionControlStatus' message to queue. Delay is: {delay}. Target date is: {triggeringTime}");
                var messageToQueue = new DeviceQueueMessage()
                {
                    Attributes = new Dictionary<string, string>()
                    {
                        { ApplicationConstants.QueuedMessageDefaultKey, ((int)status).ToString() },
                    },
                    DeviceIdentifier = device.Identifier,
                    MessageTypeId = (int)QueuedMessages.SetMotionControlStatus,
                };
                var messageJson = JsonSerializer.Serialize(messageToQueue);
                var res = await _queueClient.SendMessage(messageJson, delay);

                await _deviceRepository.SetQueuedCommand(device.Id, new DeviceQueuedCommand()
                {
                    Type = (int)QueuedMessages.SetMotionControlStatus,
                    Message = messageJson,
                    MessageId = res.MessageId,
                    PopReceipt = res.PopReceipt,
                    Created = _dateService.CurrentTime(),
                    CreatedUtc = _dateService.LocalToUtc(_dateService.CurrentTime()),
                    Scheduled = _dateService.UtcToLocal(res.ScheludedToExecuteUtc),
                    ScheduledUtc = res.ScheludedToExecuteUtc,
                }, true);

                var attributes = await _deviceRepository.GetDeviceAttributes(device.Id);
                return _mapper.Map<List<DeviceAttributeDto>>(attributes);
            }

            var message = $"MOTIONCONTROLSTATUS:{(int)status}";
            _logger.LogInformation($"Sending message: '{message}' to device: {device.Id}");
            await _messageService.SendMessageToDevice(device.DeviceIdentifier, message);
            await _deviceRepository.UpdateDeviceAttribute(device.Id, (int)DeviceAttributeTypes.MotionControlStatus, ((int)status).ToString(), false);
            await _deviceRepository.AddEvent(device.Id, DeviceEventTypes.SetMotionControlStatus, $"Motion control status set to: {(int)status} ({status.ToString()})", true, null);

            var updatedAttributes = await _deviceRepository.GetDeviceAttributes(device.Id);
            return _mapper.Map<List<DeviceAttributeDto>>(updatedAttributes);
        }

        public async Task<List<DeviceAttributeDto>> SetMotionControlDelay(Guid identifier, long delayMs, DateTime? triggeringTime = null)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [identifier] })).FirstOrDefault() ?? throw new EntityNotFoundException($"Device with identifier: '{identifier}' not found.");
            if (device.IsVirtual)
            {
                throw new InvalidOperationException($"Cannot send messages to a virtual device ({device.Id})");
            }

            if (triggeringTime != null)
            {
                ValidateTriggeringTime(triggeringTime.Value);
                var delay = triggeringTime.Value - _dateService.CurrentTime();
                _logger.LogInformation($"Setting 'SetMotionControlDelay' message to queue. Delay is: {delay}. Target date is: {triggeringTime}");
                var messageToQueue = new DeviceQueueMessage()
                {
                    Attributes = new Dictionary<string, string>()
                    {
                        { ApplicationConstants.QueuedMessageDefaultKey, delayMs.ToString() },
                    },
                    DeviceIdentifier = device.Identifier,
                    MessageTypeId = (int)QueuedMessages.SetMotionControlOnDelay,
                };
                var messageJson = JsonSerializer.Serialize(messageToQueue);
                var res = await _queueClient.SendMessage(messageJson, delay);

                await _deviceRepository.SetQueuedCommand(device.Id, new DeviceQueuedCommand()
                {
                    Type = (int)QueuedMessages.SetMotionControlOnDelay,
                    Message = messageJson,
                    MessageId = res.MessageId,
                    PopReceipt = res.PopReceipt,
                    Created = _dateService.CurrentTime(),
                    CreatedUtc = _dateService.LocalToUtc(_dateService.CurrentTime()),
                    Scheduled = _dateService.UtcToLocal(res.ScheludedToExecuteUtc),
                    ScheduledUtc = res.ScheludedToExecuteUtc,
                }, true);

                var attributes = await _deviceRepository.GetDeviceAttributes(device.Id);
                return _mapper.Map<List<DeviceAttributeDto>>(attributes);
            }

            var message = $"MOTIONCONTROLDELAY: {delayMs}";
            _logger.LogInformation($"Sending message: '{message}' to device: {device.Id}");
            await _messageService.SendMessageToDevice(device.DeviceIdentifier, message);
            await _deviceRepository.UpdateDeviceAttribute(device.Id, (int)DeviceAttributeTypes.OnDelay, delayMs.ToString(), false);
            await _deviceRepository.AddEvent(device.Id, DeviceEventTypes.SetMotionControlStatus, $"Motion control delay set to: {(int)delayMs} ms", true, null);

            var updatedAttributes = await _deviceRepository.GetDeviceAttributes(device.Id);
            return _mapper.Map<List<DeviceAttributeDto>>(updatedAttributes);
        }

        public async Task SendAttributesToDevice(Guid identifier, string? message = null)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }

            var device = (await _deviceRepository.GetDevices(new GetDevicesModel() { Identifiers = [identifier] })).FirstOrDefault();

            if (device == null)
            {
                throw new EntityNotFoundException($"Device with identifier: '{identifier}' not found.");
            }

            var attributes = await _deviceRepository.GetDeviceAttributes(device.Id);
            _logger.LogInformation($"Sending {attributes.Count} attributes to device: {device.Id} ({identifier})");
            if (!attributes.Any())
            {
                _logger.LogInformation($"No attributes found for device {device.Id} ({identifier})");
                return;
            }
            foreach (var attribute in attributes)
            {
                var type = (DeviceAttributeTypes)attribute.TypeId;
                if (string.IsNullOrEmpty(attribute.Value))
                {
                    _logger.LogInformation($"Skipping attribute type '{type}' for device {device.Id} ({identifier}) - empty value");
                    continue;
                }

                switch (type)
                {
                    case DeviceAttributeTypes.MotionControlStatus:
                        if (!int.TryParse(attribute.Value, out int statusValue))
                        {
                            _logger.LogError($"Failed to parse MotionControlStatus value '{attribute.Value}' for device {device.Id} ({identifier})");
                            continue;
                        }
                        MotionControlStatus status = (MotionControlStatus)statusValue;
                        _logger.LogInformation($"Sending MotionControlStatus '{status}' ({statusValue}) to device {device.Id} ({identifier})");
                        await SetMotionControlStatus(identifier, status);
                        break;

                    case DeviceAttributeTypes.OnDelay:
                        if (!long.TryParse(attribute.Value, out long delayMs))
                        {
                            _logger.LogError($"Failed to parse OnDelay value '{attribute.Value}' for device {device.Id} ({identifier})");
                            continue;
                        }
                        _logger.LogInformation($"Sending OnDelay '{delayMs}' ms to device {device.Id} ({identifier})");
                        await SetMotionControlDelay(identifier, delayMs);
                        break;

                    default:
                        _logger.LogInformation($"Skipping unknown attribute type '{type}' for device {device.Id} ({identifier})");
                        continue;
                }
            }

            var messageToSend = string.IsNullOrEmpty(message) ? "Sent device attributes to device" : message;
            await _deviceRepository.AddEvent(device.Id, DeviceEventTypes.SendAttributes, messageToSend, true, null);
            _logger.LogInformation($"Completed sending attributes to device {device.Id} ({identifier})");
        }

        public async Task<DeviceQueuedCommandDto> UpdateQueuedCommandSchedule(UpdateQueuedCommand model)
        {
            if (!_userService.HasAccessToDevice(model.DeviceIdentifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            ValidateTriggeringTime(model.NewScheduledTime);

            _logger.LogInformation($"Updating queued command schedule with MessageId: {model.MessageId} for device: {model.DeviceIdentifier} to {model.NewScheduledTime}");

            var device = (await _deviceRepository.GetDevices(new GetDevicesModel()
            {
                Identifiers = [model.DeviceIdentifier],
                OnlyVisible = false
            })).FirstOrDefault() ?? throw new EntityNotFoundException($"Device with identifier: '{model.DeviceIdentifier}' not found.");

            var commands = await _deviceRepository.GetQueuedCommands(new GetQueuedCommandsModel()
            {
                DeviceIds = [device.Id],
                MessageIds = [model.MessageId],
                IsExecuted = false
            });

            var command = commands.FirstOrDefault();
            if (command == null)
            {
                throw new EntityNotFoundException($"Queued command with MessageId: '{model.MessageId}' not found for device: '{model.DeviceIdentifier}'.");
            }

            if (string.IsNullOrEmpty(command.PopReceipt))
            {
                throw new InvalidOperationException($"PopReceipt is missing for MessageId: '{model.MessageId}'");
            }

            if (command.ExecutedAt != null)
            {
                throw new InvalidOperationException($"Cannot update schedule for already executed command with MessageId: '{model.MessageId}'");
            }

            var currentTime = _dateService.CurrentTime();
            var delay = model.NewScheduledTime - currentTime;

            var queueUpdateResult = await _queueClient.UpdateMessageVisibility(command.MessageId, command.PopReceipt, delay);

            command.Scheduled = model.NewScheduledTime;
            command.ScheduledUtc = _dateService.LocalToUtc(model.NewScheduledTime);
            command.PopReceipt = queueUpdateResult.PopReceipt;

            await _deviceRepository.SetQueuedCommand(device.Id, command, true);

            _logger.LogInformation($"Successfully updated queued command schedule with MessageId: {model.MessageId} for device: {model.DeviceIdentifier}");

            return _mapper.Map<DeviceQueuedCommandDto>(command);
        }

        public async Task RemoveQueuedCommand(Guid deviceIdentifier, string messageId)
        {
            if (!_userService.HasAccessToDevice(deviceIdentifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            _logger.LogInformation($"Removing queued command with MessageId: {messageId} for device: {deviceIdentifier}");
            
            var device = (await _deviceRepository.GetDevices(new GetDevicesModel()
            {
                Identifiers = [deviceIdentifier],
                OnlyVisible = false
            })).FirstOrDefault() ?? throw new EntityNotFoundException($"Device with identifier: '{deviceIdentifier}' not found.");

            var commands = await _deviceRepository.GetQueuedCommands(new GetQueuedCommandsModel()
            {
                DeviceIds = [device.Id],
                MessageIds = [messageId]
            });

            var command = commands.FirstOrDefault();
            if (command == null)
            {
                throw new EntityNotFoundException($"Queued command with MessageId: '{messageId}' not found for device: '{deviceIdentifier}'.");
            }

            if (string.IsNullOrEmpty(command.PopReceipt))
            {
                throw new InvalidOperationException($"PopReceipt is missing for MessageId: '{messageId}'");
            }

            await _queueClient.DeleteMessage(command.MessageId, command.PopReceipt);

            command.IsRemoved = true;
            await _deviceRepository.SetQueuedCommand(command.DeviceId, command, true);

            _logger.LogInformation($"Successfully removed queued command with MessageId: {messageId} for device: {deviceIdentifier}");
        }

        public async Task<DeviceQueuedCommandDto> CopyExecutedQueuedCommand(CopyQueuedCommand model)
        {
            if (!_userService.HasAccessToDevice(model.DeviceIdentifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            if (model.ScheduledTime.HasValue)
            {
                ValidateTriggeringTime(model.ScheduledTime.Value);
            }

            _logger.LogInformation($"Copying executed queued command with MessageId: {model.MessageId} for device: {model.DeviceIdentifier} with new schedule: {model.ScheduledTime?.ToString() ?? "immediate"}");

            var device = (await _deviceRepository.GetDevices(new GetDevicesModel()
            {
                Identifiers = [model.DeviceIdentifier],
                OnlyVisible = false
            })).FirstOrDefault() ?? throw new EntityNotFoundException($"Device with identifier: '{model.DeviceIdentifier}' not found.");

            var commands = await _deviceRepository.GetQueuedCommands(new GetQueuedCommandsModel()
            {
                DeviceIds = [device.Id],
                MessageIds = [model.MessageId],
                IsExecuted = true
            });

            var originalCommand = commands.FirstOrDefault();
            if (originalCommand == null)
            {
                throw new EntityNotFoundException($"Executed queued command with MessageId: '{model.MessageId}' not found for device: '{model.DeviceIdentifier}'.");
            }

            if (originalCommand.ExecutedAt == null)
            {
                throw new InvalidOperationException($"Cannot copy command that has not been executed. MessageId: '{model.MessageId}'");
            }

            // Calculate delay for the new scheduled time (null means execute immediately)
            var currentTime = _dateService.CurrentTime();
            TimeSpan delay = model.ScheduledTime.HasValue 
                ? model.ScheduledTime.Value - currentTime 
                : TimeSpan.Zero;

            // Send the message to the queue with the new schedule
            var queueResult = await _queueClient.SendMessage(originalCommand.Message, delay);

            // Create new database entry with the new MessageId from the queue and link to original command
            var newCommand = new DeviceQueuedCommand()
            {
                Type = originalCommand.Type,
                Message = originalCommand.Message,
                MessageId = queueResult.MessageId,
                PopReceipt = queueResult.PopReceipt,
                Created = _dateService.CurrentTime(),
                CreatedUtc = _dateService.LocalToUtc(_dateService.CurrentTime()),
                Scheduled = _dateService.UtcToLocal(queueResult.ScheludedToExecuteUtc),
                ScheduledUtc = queueResult.ScheludedToExecuteUtc,
                OriginalId = originalCommand.Id
            };

            await _deviceRepository.SetQueuedCommand(device.Id, newCommand, true);

            _logger.LogInformation($"Successfully copied queued command. Original MessageId: {model.MessageId}, New MessageId: {queueResult.MessageId} for device: {model.DeviceIdentifier}");

            return _mapper.Map<DeviceQueuedCommandDto>(newCommand);
        }

        public async Task AckQueuedCommand(Guid identifier, string messageId, DateTime? date)
        {
            if (!_userService.HasAccessToDevice(identifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }
            _logger.LogInformation($"Acknowledging queued command with MessageId: {messageId} for device: {identifier}");

            var device = (await _deviceRepository.GetDeviceInfo(new GetDevicesModel()
            {
                Identifiers = [identifier],
                OnlyVisible = false
            })).FirstOrDefault() ?? throw new EntityNotFoundException($"Device with identifier: '{identifier}' not found.");

            var commands = await _deviceRepository.GetQueuedCommands(new GetQueuedCommandsModel()
            {
                DeviceIdentifiers = [identifier],
                MessageIds = [messageId]
            });

            var command = commands.FirstOrDefault();
            if (command == null)
            {
                _logger.LogInformation($"Queued command with MessageId: {messageId} for device: {device.Device.Id} not found");
                return;
            }

            if (date != null)
            {

                command.ExecutedAt = date.Value;
                command.ExecutedAtUtc = _dateService.LocalToUtc(date.Value);
            }
            else
            {
                command.IsRemoved = true; // Indicates error
            }

            await _deviceRepository.SetQueuedCommand(device.Device.Id, command, true);

            _logger.LogInformation($"Successfully acknowledged queued command with MessageId: {messageId} for device: {device.Device.Id}. ExecutedAt: {date}");
        }

        private void ValidateTriggeringTime(DateTime target)
        {
            var compareDate = _dateService.CurrentTime();
            if (target < compareDate)
            {
                throw new ArgumentException($"Invalid triggering time: {target}. Current date: {compareDate}");
            }
        }
    }
}
