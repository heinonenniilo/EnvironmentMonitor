using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Models;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IDeviceEmailService
    {
        Task<DeviceEmailTemplateDto?> GetEmailTemplate(EmailTemplateTypes templateType);
        Task<DeviceEmailTemplateDto?> GetEmailTemplateByIdentifier(Guid identifier);
        Task<List<DeviceEmailTemplateDto>> GetAllEmailTemplates();
        Task<DeviceEmailTemplateDto> UpdateEmailTemplate(UpdateDeviceEmailTemplateDto model);
        Task SendDeviceEmail(Guid deviceIdentifier, EmailTemplateTypes templateType, Dictionary<string, string>? replaceTokens = null);

        Task QueueDeviceStatusEmail(SetDeviceStatusModel model, DeviceStatus currentStatus, DeviceStatus? previousStatus, bool saveChanges);
    }
}
