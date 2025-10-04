using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Domain.Models.ReturnModel;
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
        Task<List<MeasurementExtended>> GetMeasurements(GetMeasurementsModel model);
        public Task<MeasurementType?> GetMeasurementType(int id);
        Task<IList<Measurement>> AddMeasurements(List<Measurement> measurements, bool saveChanges = true, DeviceMessage? deviceMessage = null);
        Task<DeviceMessage?> GetDeviceMessage(string messageIdentifier, int deviceId);
        Task<DeviceMessage> AddDeviceMessage(DeviceMessage deviceMessage, bool saveChanges);
        Task<List<PublicSensor>> GetPublicSensors();

        public Task<IEnumerable<Measurement>> Get(
            Expression<Func<Measurement, bool>> filter = null,
            Func<IQueryable<Measurement>, IOrderedQueryable<Measurement>> orderBy = null,
            string includeProperties = "");
    } 
}
