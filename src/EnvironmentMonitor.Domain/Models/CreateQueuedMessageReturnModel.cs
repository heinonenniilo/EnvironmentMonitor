using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class CreateQueuedMessageReturnModel
    {
        public required string MessageId { get; set; }
        public DateTime ScheludedToExecute { get; set; }
    }
}
