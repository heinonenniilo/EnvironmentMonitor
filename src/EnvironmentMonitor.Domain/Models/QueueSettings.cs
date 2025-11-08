using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class QueueSettings
    {
        public string? QueueServiceUri { get; set; }
        public string? DefaultQueueName { get; set; }
    }
}
