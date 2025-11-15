using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models.GetModels
{
    public class GetQueuedCommandsModel
    {
        public List<int>? Ids { get; set; }
        public List<int>? DeviceIds { get; set; }
        public List<Guid>? DeviceIdentifiers { get; set; }
        public List<string>? MessageIds { get; set; }
        public DateTime? ScheduledFrom { get; set; }
        public DateTime? ScheduledTo { get; set; }
        public bool? IsExecuted { get; set; }
        public int? Limit { get; set; } = 50;
    }
}
