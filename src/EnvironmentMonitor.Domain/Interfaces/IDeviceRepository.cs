using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Models;
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
        Task<Device?> GetDeviceByIdentifier(string deviceId);

        Task<List<Device>> GetDevices(List<int>? ids = null, bool onlyVisible = true);
        Task<List<Device>> GetDevices(List<string>? identifiers = null, bool onlyVisible = true);
        Task<List<Device>> GetDevicesByLocation(List<int> locationIds);

        Task<List<DeviceInfo>> GetDeviceInfo(List<int>? ids, bool onlyVisible, bool getAttachments = false);
        Task<List<DeviceInfo>> GetDeviceInfo(List<string>? identifiers, bool onlyVisible, bool getAttachments = false);

        Task<Attachment> GetAttachment(int deviceId, Guid attachmentIdentifier);

        Task<List<DeviceEvent>> GetDeviceEvents(int id);
        Task<List<DeviceEvent>> GetDeviceEvents(string deviceIdentifier);

        Task<IEnumerable<Sensor>> GetSensorsByDeviceIdsAsync(List<int> deviceId);
        Task<IEnumerable<Sensor>> GetSensorsByDeviceIdentifiers(List<string> deviceIdentifiers);
        public Task<Sensor?> GetSensor(int deviceId, int sensorIdInternal);

        public Task<DeviceEvent> AddEvent(int deviceId, DeviceEventTypes type, string message, bool saveChanges, DateTime? datetimeUtc);
        public Task AddAttachment(int deviceId, Attachment attachment, bool saveChanges);

        public Task<DeviceInfo> AddOrUpdate(Device device, bool saveChanges);

        public Task DeleteAttachment(int deviceId, Guid attachmentIdentifier, bool saveChanges);
        public Task SetDefaultImage(int deviceId, Guid attachmentIdentifier);

        public Task SetStatus(SetDeviceStatusModel status, bool saveChanges);
        public Task SaveChanges();

        public Task<List<DeviceStatus>> GetDevicesStatus(GetDeviceStatusModel model);
    }
}
