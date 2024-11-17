using EnvironmentMonitor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IMeasurementRepository
    {
        Task<IEnumerable<Sensor>> GetSensorsByDeviceIdAsync(string deviceId);
        Task<IEnumerable<Measurement>> GetMeasurementsBySensorId(int sensorId);
        Task<Device?> GetDeviceByIdAsync(string deviceId);
        public Task<Sensor?> GetSensor(int deviceId, int sensorIdInternal);
        public Task<MeasurementType?> GetMeasurementType(int id);
        Task<IList<Measurement>> AddMeasurements(List<Measurement> measurements);
    }
}
