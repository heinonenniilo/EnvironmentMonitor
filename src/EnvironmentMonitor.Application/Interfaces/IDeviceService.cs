using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Interfaces
{
    public interface IDeviceService
    {
        public Task Reboot(string deviceIdentifier);
        public Task SetMotionControlStatus(string deviceIdentifier, MotionControlStatus status);
        public Task SetMotionControlDelay(string deviceIdentifier, long delayMs);
        public Task<DeviceDto> GetDevice(string deviceIdentifier, AccessLevels accessLevel);
        public Task<List<DeviceDto>> GetDevices();
        public Task<List<DeviceInfoDto>> GetDeviceInfos(bool onlyVisible, List<string>? identifiers);
        public Task<List<SensorDto>> GetSensors(List<string> deviceIdentifiers);
        public Task<List<SensorDto>> GetSensors(List<int> deviceIds);
        public Task<SensorDto?> GetSensor(int deviceId, int sensorIdInternal, AccessLevels accessLevel);
        public Task AddEvent(int deviceId, DeviceEventTypes type, string message, bool saveChanges, DateTime? datetimeUtc = null);
        public Task SetDefaultImage(string deviceIdentifier, Stream fileStream, string fileName);
        public Task<List<DeviceEventDto>> GetDeviceEvents(string identifier);

        public Task<AttachmentInfoModel?> GetDefaultImage(string deviceIdentifier);
    }
}
