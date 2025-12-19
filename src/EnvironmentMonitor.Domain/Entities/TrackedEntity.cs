using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class TrackedEntity
    {
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public required DateTime Created { get; set; }

        public DateTime? Updated { get; set; }
        public DateTime? UpdatedUtc { get; set; }
    }
}