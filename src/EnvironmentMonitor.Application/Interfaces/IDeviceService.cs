using EnvironmentMonitor.Application.DTOs;
using EnvironmentMonitor.Domain.Enums;
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
        public Task<DeviceDto> GetDevice(string deviceIdentifier, AccessLevels accessLevel);
        public Task<List<DeviceDto>> GetDevices();
        public Task<List<SensorDto>> GetSensors(List<string> deviceIdentifiers);
        public Task<List<SensorDto>> GetSensors(List<int> deviceIds);
        public Task<SensorDto> GetSensor(int deviceId, int sensorIdInternal, AccessLevels accessLevel);
        public Task AddEvent(int deviceId, DeviceEventTypes type, string message, bool saveChanges);
    }
}
