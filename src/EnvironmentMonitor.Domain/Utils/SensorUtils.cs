using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Models.ReturnModel;
using System.Linq.Expressions;

namespace EnvironmentMonitor.Domain.Utils
{
    public static class SensorUtils
    {
        public static Expression<Func<Sensor, SensorExtended>> ToSensorExtendedExpression =>
            x => new SensorExtended
            {
                Id = x.Id,
                SensorId = x.SensorId,
                Name = x.Name,
                Identifier = x.Identifier,
                DeviceId = x.DeviceId,
                DeviceIdentifier = x.Device.Identifier,
                ScaleMin = x.ScaleMin,
                ScaleMax = x.ScaleMax,
                TypeId = x.TypeId,
                IsVirtual = x.IsVirtual,
                AggregationType = x.AggregationType,
                Active = x.Active,
                VirtualSensorRows = x.VirtualSensorRows,
                VirtualSensorRowValues = x.VirtualSensorRowValues,
                Created = x.Created,
                CreatedUtc = x.CreatedUtc,
                Updated = x.Updated,
                UpdatedUtc = x.UpdatedUtc,
            };

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
