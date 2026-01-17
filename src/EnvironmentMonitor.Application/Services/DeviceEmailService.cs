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
using System.Reflection;
using System.Text.Json;

namespace EnvironmentMonitor.Application.Services
{
    public class DeviceEmailService : IDeviceEmailService
    {
        private readonly IEmailRepository _deviceEmailRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IUserService _userService;
        private readonly IEmailClient _emailClient;
        private readonly IMapper _mapper;
        private readonly ILogger<DeviceEmailService> _logger;
        private readonly IQueueClient _queueClient;
        private readonly IDateService _dateService;
        private readonly ApplicationSettings _applicationSettings;

        public DeviceEmailService(
            IEmailRepository deviceEmailRepository,
            IDeviceRepository deviceRepository,
            IUserService userService,
            IEmailClient emailClient,
            IQueueClient queueClient,   
            IMapper mapper,

            ILogger<DeviceEmailService> logger,
            IDateService dateService,
            ApplicationSettings applicationSettings)
        {
            _deviceEmailRepository = deviceEmailRepository;
            _deviceRepository = deviceRepository;
            _userService = userService;
            _emailClient = emailClient;
            _mapper = mapper;
            _logger = logger;
            _queueClient = queueClient;
            _dateService = dateService;
            _applicationSettings = applicationSettings;
        }

        public async Task<DeviceEmailTemplateDto?> GetEmailTemplate(EmailTemplateTypes templateType)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException("Only administrators can access email templates.");
            }

            _logger.LogInformation($"Fetching email template: {templateType}");

            var template = await _deviceEmailRepository.GetEmailTemplate(templateType);
            
            return template != null ? _mapper.Map<DeviceEmailTemplateDto>(template) : null;
        }

        public async Task<DeviceEmailTemplateDto?> GetEmailTemplateByIdentifier(Guid identifier)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException("Only administrators can access email templates.");
            }

            _logger.LogInformation($"Fetching email template with identifier: {identifier}");

            var template = await _deviceEmailRepository.GetEmailTemplateByIdentifier(identifier);
            
            return template != null ? _mapper.Map<DeviceEmailTemplateDto>(template) : null;
        }

        public async Task<List<DeviceEmailTemplateDto>> GetAllEmailTemplates()
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException("Only administrators can access email templates.");
            }

            _logger.LogInformation("Fetching all email templates");

            var templates = await _deviceEmailRepository.GetAllEmailTemplates();
            
            return _mapper.Map<List<DeviceEmailTemplateDto>>(templates);
        }

        public async Task<DeviceEmailTemplateDto> UpdateEmailTemplate(UpdateDeviceEmailTemplateDto model)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException("Only administrators can update email templates.");
            }

            _logger.LogInformation($"Updating email template with identifier: {model.Identifier}");

            var updatedTemplate = await _deviceEmailRepository.UpdateEmailTemplate(
                model.Identifier,
                model.Title,
                model.Message,
                true);

            _logger.LogInformation($"Successfully updated email template with identifier: {model.Identifier}");

            return _mapper.Map<DeviceEmailTemplateDto>(updatedTemplate);
        }

        public async Task SendDeviceEmail(Guid deviceIdentifier, EmailTemplateTypes templateType, Dictionary<string, string>? replaceTokens = null)
        {
            if (!_userService.HasAccessToDevice(deviceIdentifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }
            _logger.LogInformation($"Preparing to send email for device: {deviceIdentifier} using template: {templateType}");

            var device = (await _deviceRepository.GetDevices(new GetDevicesModel()
            {
                Identifiers = [deviceIdentifier],
                GetLocation = true,
                GetContacts = true
            })).FirstOrDefault();
            
            if (device == null)
            {
                _logger.LogError($"Device with identifier: '{deviceIdentifier}' not found.");
                throw new EntityNotFoundException($"Device with identifier: '{deviceIdentifier}' not found.");
            }

            var template = await _deviceEmailRepository.GetEmailTemplate(templateType);
            if (template == null)
            {
                _logger.LogWarning($"Email template '{templateType}' not found.");
                throw new EntityNotFoundException($"Email template '{templateType}' not found.");
            }
            
            var tokens = new Dictionary<string, string>
            {
                { "{Location}", $"{device.Location.Name}"},
                { "{DeviceName}", $"{device.Name}"},
                { "{DeviceIdentifier}", device.DeviceIdentifier },
                { "{DisplayName}", $"{device.Location.Name} -  {device.Name}"},
                { ApplicationConstants.QueuedMessageDeviceLink, BuildDeviceUrl(device.Identifier) },
                { ApplicationConstants.QueuedMessageApplicationBaseUrlKey, _applicationSettings.BaseUrl}
            };

            if (replaceTokens != null)
            {
                foreach (var token in replaceTokens)
                {
                    tokens[token.Key] = token.Value;
                }
            }
            
            _logger.LogInformation($"Sending email for device '{device.Name}'. Subject: {template.Title}");
            try
            {
                var emailOptions = new SendEmailOptions
                {
                    ToAddresses = device.Contacts.Select(x => x.Email).ToList(),
                    Subject = template.Title ?? string.Empty,
                    HtmlContent = template.Message ?? string.Empty,
                    ReplaceTokens = tokens
                };
                
                await _emailClient.SendEmailAsync(emailOptions);
                _logger.LogInformation($"Email sent successfully for device '{device.Name}' (Template: {templateType})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email for device '{device.Name}' using template '{templateType}'");
                throw;
            }
        }

        public async Task QueueDeviceStatusEmail(SetDeviceStatusModel model, DeviceStatus currentStatus, DeviceStatus? previousStatus, bool saveChanges)
        {
            if (!_userService.HasAccessToDevice(model.Idenfifier, AccessLevels.Write))
            {
                throw new UnauthorizedAccessException();
            }

            var device = (await _deviceRepository.GetDevices(new GetDevicesModel()
            {
                Identifiers = [model.Idenfifier],
                OnlyVisible = false,
                GetLocation = true
            })).FirstOrDefault();

            if (device == null)
            {
                _logger.LogError($"Device with identifier '{model.Idenfifier}' not found for queuing email.");
                return;
            }
            var timeStamp = model.TimeStamp ?? _dateService.CurrentTime();
            _logger.LogInformation($"Queuing device email for devie {device.Name} ({device.Id}). Type: {currentStatus.Status}");
            EmailTemplateTypes messageType = currentStatus.Status ? EmailTemplateTypes.ConnectionOk : EmailTemplateTypes.ConnectionLost;

            var attributesToAdd = new Dictionary<string, string>()
                    {
                        { ApplicationConstants.QueuedMessageDefaultKey, ((int)messageType).ToString() },
                        { ApplicationConstants.QueuedMessageTimesStampKey, _dateService.FormatDateTime(currentStatus.TimeStamp) },
                        { ApplicationConstants.QueuedMessageTimesStampPreviousKey, previousStatus != null ? _dateService.FormatDateTime(previousStatus.TimeStamp) : string.Empty }
                    };

            var messageToQueue = new DeviceQueueMessage()
            {
                Attributes = attributesToAdd,
                DeviceIdentifier = model.Idenfifier,
                MessageTypeId = (int)QueuedMessages.SendDeviceEmail,
            };
            var messageJson = JsonSerializer.Serialize(messageToQueue);
            var res = await _queueClient.SendMessage(messageJson);

            await _deviceRepository.SetQueuedCommand(device.Id, new DeviceQueuedCommand()
            {
                Type = (int)QueuedMessages.SendDeviceEmail,
                Message = messageJson,
                MessageId = res.MessageId,
                PopReceipt = res.PopReceipt,
                Created = _dateService.CurrentTime(),
                CreatedUtc = _dateService.LocalToUtc(_dateService.CurrentTime()),
                Scheduled = _dateService.UtcToLocal(res.ScheludedToExecuteUtc),
                ScheduledUtc = res.ScheludedToExecuteUtc,

            }, saveChanges);
        }

        private string BuildDeviceUrl(Guid deviceIdentifier)
        {
            var baseUrl = _applicationSettings.BaseUrl.TrimEnd('/');
            var uriBuilder = new UriBuilder(baseUrl)
            {
                Path = $"/devices/{deviceIdentifier}"
            };
            return uriBuilder.Uri.ToString();
        }
    }
}
