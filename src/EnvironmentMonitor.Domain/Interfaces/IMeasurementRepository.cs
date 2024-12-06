using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IMeasurementRepository
    {
        Task<IEnumerable<Sensor>> GetSensorsByDeviceIdAsync(int deviceId);
        Task<IEnumerable<Sensor>> GetSensorsByDeviceIdentifiers(List<string> deviceIdentifiers);
        Task<IEnumerable<Measurement>> GetMeasurements(GetMeasurementsModel model);
        Task<Device?> GetDeviceByIdentifier(string deviceId);
        Task<List<Device>> GetDevices(List<int>? ids = null);
        public Task<Sensor?> GetSensor(int deviceId, int sensorIdInternal);
        public Task<MeasurementType?> GetMeasurementType(int id);
        Task<IList<Measurement>> AddMeasurements(List<Measurement> measurements);

        public Task<IEnumerable<Measurement>> Get(
            Expression<Func<Measurement, bool>> filter = null,
            Func<IQueryable<Measurement>, IOrderedQueryable<Measurement>> orderBy = null,
            string includeProperties = "");
    }
}
