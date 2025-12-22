using AutoMapper;
using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Exceptions;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Models.GetModels;
using Microsoft.Extensions.Logging;

namespace EnvironmentMonitor.Application.Services
{
    public class DeviceEmailService : IDeviceEmailService
    {
        private readonly IDeviceEmailRepository _deviceEmailRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IUserService _userService;
        private readonly IEmailClient _emailClient;
        private readonly IMapper _mapper;
        private readonly ILogger<DeviceEmailService> _logger;

        public DeviceEmailService(
            IDeviceEmailRepository deviceEmailRepository,
            IDeviceRepository deviceRepository,
            IUserService userService,
            IEmailClient emailClient,
            IMapper mapper,
            ILogger<DeviceEmailService> logger)
        {
            _deviceEmailRepository = deviceEmailRepository;
            _deviceRepository = deviceRepository;
            _userService = userService;
            _emailClient = emailClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DeviceEmailTemplateDto?> GetEmailTemplate(DeviceEmailTemplateTypes templateType)
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

        public async Task SendDeviceEmail(Guid deviceIdentifier, DeviceEmailTemplateTypes templateType, Dictionary<string, string>? replaceTokens = null)
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
                { "{DeviceName}", $"{device.Location.Name} - {device.Name}"},
                { "{DeviceIdentifier}", device.DeviceIdentifier }
            };

            if (replaceTokens != null)
            {
                foreach (var token in replaceTokens)
                {
                    tokens[token.Key] = token.Value;
                }
            }
            
            var title = template.Title ?? string.Empty;
            var message = template.Message ?? string.Empty;

            foreach (var token in tokens)
            {
                title = title.Replace(token.Key, token.Value);
                message = message.Replace(token.Key, token.Value);
            }
            
            _logger.LogInformation($"Sending email for device '{device.Name}'. Subject: {title}");
            try
            {
                var emailOptions = new SendEmailOptions
                {
                    ToAddresses = device.Contacts.Select(x => x.Email).ToList(),
                    Subject = title,
                    HtmlContent = message
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
    }
}
