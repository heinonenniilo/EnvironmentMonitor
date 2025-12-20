using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class GetDeviceStatusModel
    {
        public required List<Guid> DeviceIdentifiers { get; set; }
        public DateTime From { get; set; } = DateTime.UtcNow.AddDays(-4);
        public DateTime? To { get; set; }
        public bool LatestOnly { get; set; } = false;
    }
}
