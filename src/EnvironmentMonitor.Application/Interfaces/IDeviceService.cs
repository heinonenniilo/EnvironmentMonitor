using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Models.AddModels;
using EnvironmentMonitor.Domain.Models.GetModels;
using EnvironmentMonitor.Domain.Models.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IDeviceService
    {
        public Task Reboot(Guid identifier);
        public Task<List<DeviceAttributeDto>> SetMotionControlStatus(Guid identifier, MotionControlStatus status, DateTime? triggeringTime = null);
        public Task<List<DeviceAttributeDto>> SetMotionControlDelay(Guid identifier, long delayMs, DateTime? triggeringTime = null);
        public Task SendAttributesToDevice(Guid identifier, string? message = null);

        public Task<DeviceDto> GetDevice(string deviceIdentifier, AccessLevels accessLevel);
        public Task<DeviceDto> GetDevice(Guid identifier, AccessLevels accessLevel);

        public Task<List<DeviceDto>> GetDevices(bool onlyVisible, bool getLocation);
        public Task<List<DeviceInfoDto>> GetDeviceInfos(bool onlyVisible, List<Guid>? identifiers, bool getAttachments = false, bool getLocation = false, bool getAttributes = false, bool getContacts = false);
        public Task<List<SensorDto>> GetSensors(List<Guid> identifiers);
        public Task<List<SensorDto>> GetSensors(List<int> deviceIds);
        public Task<SensorDto?> GetSensor(int deviceId, int sensorIdInternal, AccessLevels accessLevel);
        public Task<PaginatedResult<DeviceMessageDto>> GetDeviceMessages(GetDeviceMessagesModel model);
        public Task<List<DeviceQueuedCommandDto>> GetQueuedCommands(GetQueuedCommandsModel model);

        public Task<DeviceStatusModel> GetDeviceStatus(GetDeviceStatusModel model);
        public Task AddAttachment(UploadDeviceAttachmentModel fileModel);
        public Task DeleteAttachment(Guid identifier, Guid attachmentIdentifier);
        public Task<AttachmentDownloadModel?> GetAttachment(Guid identifier, Guid attachmentIdentifier);
        public Task<AttachmentDownloadModel?> GetDefaultImage(Guid identifier);
        public Task SetDefaultImage(Guid identifier, Guid attachmentGuid);

        public Task AddEvent(int deviceId, DeviceEventTypes type, string message, bool saveChanges, DateTime? datetimeUtc = null);
        public Task<List<DeviceEventDto>> GetDeviceEvents(Guid identifier);
        public Task SetStatus(SetDeviceStatusModel model, bool saveChanges);

        public Task<DeviceInfoDto> UpdateDevice(UpdateDeviceDto model);
        
        public Task<DeviceContactDto> AddDeviceContact(AddOrUpdateDeviceContactDto model);
        public Task<DeviceContactDto> UpdateDeviceContact(AddOrUpdateDeviceContactDto model);
        public Task DeleteDeviceContact(AddOrUpdateDeviceContactDto model);
    }
}
