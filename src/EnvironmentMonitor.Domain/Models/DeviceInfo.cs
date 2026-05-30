using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Models.ReturnModel;

namespace EnvironmentMonitor.Domain.Models
{
    public class DeviceInfo
    {
        public Device Device { get; set; }
        public DateTime? OnlineSince { get; set; }
        public DateTime? RebootedOn { get; set; }
        public DateTime? LastMessage { get; set; }
        public Guid? DefaultImageGuid { get; set; }

        public List<SensorExtended> Sensors { get; set; } = [];
    }
}
