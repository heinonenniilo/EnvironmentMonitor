using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class DeviceQueuedCommandType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<DeviceQueuedCommand> QueuedCommands { get; set; } = new List<DeviceQueuedCommand>();
    }
}
