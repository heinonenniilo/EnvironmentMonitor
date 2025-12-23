using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Models.AddModels;
using EnvironmentMonitor.Domain.Models.GetModels;
using EnvironmentMonitor.Domain.Models.Pagination;
using EnvironmentMonitor.Domain.Models.ReturnModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IDeviceRepository
    {

        Task<List<Device>> GetDevices(GetDevicesModel model);
        Task<List<DeviceInfo>> GetDeviceInfo(GetDevicesModel model);
        Task<Attachment> GetAttachment(int deviceId, Guid attachmentIdentifier);
        Task<List<DeviceEvent>> GetDeviceEvents(int id);
        Task<List<SensorExtended>> GetSensors(GetSensorsModel model);
        Task<PaginatedResult<DeviceMessageExtended>> GetDeviceMessages(GetDeviceMessagesModel model);
        Task<List<DeviceQueuedCommand>> GetQueuedCommands(GetQueuedCommandsModel model);

        public Task<DeviceEvent> AddEvent(int deviceId, DeviceEventTypes type, string message, bool saveChanges, DateTime? datetimeUtc);
        public Task AddAttachment(AddDeviceAttachmentModel model);
        public Task DeleteAttachment(int deviceId, Guid attachmentIdentifier, bool saveChanges);
        public Task SetDefaultImage(int deviceId, Guid attachmentIdentifier);
        public Task SetQueuedCommand(int deviceId, DeviceQueuedCommand command, bool saveChanges);

        public Task<DeviceInfo> AddOrUpdate(Device device, bool saveChanges);
        public Task SetDeviceAttributes(int deviceId, List<DeviceAttribute> attributes, bool saveChanges);
        public Task UpdateDeviceAttribute(int deviceId, int typeId, string value, bool saveChanges);
        public Task<List<DeviceAttribute>> GetDeviceAttributes(int deviceId);
        public Task<DeviceAttribute?> GetDeviceAttribute(int deviceId, int typeId);

        public Task<DeviceStatus?> SetStatus(SetDeviceStatusModel status, bool saveChanges);
        public Task SaveChanges();

        public Task<List<DeviceStatus>> GetDevicesStatus(GetDeviceStatusModel model);
        
        public Task<DeviceContact> AddDeviceContact(int deviceId, string email, bool saveChanges);
        public Task<DeviceContact> UpdateDeviceContact(Guid identifier, string email, bool saveChanges);
        public Task DeleteDeviceContact(Guid identifier, bool saveChanges);
        public Task<DeviceContact?> GetDeviceContact(Guid identifier);
    }
}
