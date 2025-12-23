using AutoMapper;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
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
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Services
{
    public class QueuedCommandService : IQueuedCommandService
    {
        private readonly ILogger<QueuedCommandService> _logger;
        private readonly IUserService _userService;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IMapper _mapper;
        private readonly IDateService _dateService;
        private readonly IQueueClient _queueClient;

        public QueuedCommandService(
            ILogger<QueuedCommandService> logger,
            IUserService userService,
            IDeviceRepository deviceRepository,
            IMapper mapper,
            IDateService dateService,
            IQueueClient queueClient)
        {
            _logger = logger;
            _userService = userService;
            _deviceRepository = deviceRepository;
            _mapper = mapper;
            _dateService = dateService;
            _queueClient = queueClient;
        }

        public async Task<List<DeviceQueuedCommandDto>> GetQueuedCommands(Guid deviceIdentifier)
        {
            if (!_userService.HasAccessToDevice(deviceIdentifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            _logger.LogInformation($"Fetching queued commands for device: {deviceIdentifier}");

            var device = (await _deviceRepository.GetDevices(new GetDevicesModel()
            {
                Identifiers = [deviceIdentifier],
                OnlyVisible = false
            })).FirstOrDefault() ?? throw new EntityNotFoundException($"Device with identifier: '{deviceIdentifier}' not found.");

            var commands = await _deviceRepository.GetQueuedCommands(new GetQueuedCommandsModel()
            {
                DeviceIds = [device.Id]
            });

            return _mapper.Map<List<DeviceQueuedCommandDto>>(commands);
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

            // Create new database entry with the new MessageId from the queue
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
