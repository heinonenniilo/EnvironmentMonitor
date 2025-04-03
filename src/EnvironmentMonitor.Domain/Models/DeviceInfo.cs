using EnvironmentMonitor.Domain.Entities;

namespace EnvironmentMonitor.Domain.Models
{
    public class DeviceInfo
    {
        public Device Device { get; set; }
        public DateTime? OnlineSince { get; set; }
        public DateTime? RebootedOn { get; set; }
        public DateTime? LastMessage { get; set; }
    }
}
