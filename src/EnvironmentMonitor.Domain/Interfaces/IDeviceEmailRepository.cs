using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IDeviceEmailRepository
    {
        Task<DeviceEmailTemplate?> GetEmailTemplate(DeviceEmailTemplateTypes templateType);
        Task<DeviceEmailTemplate?> GetEmailTemplateByIdentifier(Guid identifier);
        Task<List<DeviceEmailTemplate>> GetAllEmailTemplates();
        Task<DeviceEmailTemplate> UpdateEmailTemplate(Guid identifier, string? title, string? message, bool saveChanges);
    }
}
