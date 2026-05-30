using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Models.ReturnModel;

namespace EnvironmentMonitor.Domain.Utils
{
    public static class SensorUtils
    {
        public static SensorExtended ToSensorExtended(this Sensor s)
        {
            return new SensorExtended
            {
                Id = s.Id,
                SensorId = s.SensorId,
                Name = s.Name,
                Identifier = s.Identifier,
                DeviceId = s.DeviceId,
                DeviceIdentifier = s.Device.Identifier,
                ScaleMin = s.ScaleMin,
                ScaleMax = s.ScaleMax,
                TypeId = s.TypeId,
                IsVirtual = s.IsVirtual,
                AggregationType = s.AggregationType,
                Active = s.Active,
                VirtualSensorRows = s.VirtualSensorRows,
                VirtualSensorRowValues = s.VirtualSensorRowValues,
                Created = s.Created,
                CreatedUtc = s.CreatedUtc,
                Updated = s.Updated,
                UpdatedUtc = s.UpdatedUtc,
            };
        }
    }
}
