using EnvironmentMonitor.Domain.Entities;
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
    }
}
