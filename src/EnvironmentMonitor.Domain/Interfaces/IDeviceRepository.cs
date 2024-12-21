using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IDeviceRepository
    {
        Task<Device?> GetDeviceByIdentifier(string deviceId);
        Task<List<Device>> GetDevices(List<int>? ids = null);
        Task<List<Device>> GetDevices(List<string>? identifiers = null);

        Task<IEnumerable<Sensor>> GetSensorsByDeviceIdsAsync(List<int> deviceId);
        Task<IEnumerable<Sensor>> GetSensorsByDeviceIdentifiers(List<string> deviceIdentifiers);
        public Task<Sensor?> GetSensor(int deviceId, int sensorIdInternal);
        public Task<DeviceEvent> AddEvent(int deviceId, DeviceEventTypes type, string message, bool saveChanges = true);
    }
}
