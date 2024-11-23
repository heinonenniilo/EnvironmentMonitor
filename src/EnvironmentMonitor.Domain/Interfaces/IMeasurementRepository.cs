using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Models;
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
        Task<IEnumerable<Measurement>> GetMeasurements(GetMeasurementsModel model);
        Task<Device?> GetDeviceByIdAsync(string deviceId);
        public Task<Sensor?> GetSensor(int deviceId, int sensorIdInternal);
        public Task<MeasurementType?> GetMeasurementType(int id);
        Task<IList<Measurement>> AddMeasurements(List<Measurement> measurements);
    }
}
