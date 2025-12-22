using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class DeviceEmailTemplate
    {
        public int Id { get; set; }
        public Guid Identifier { get; set; }
        public string? Title { get; set; }        
        public string? Message { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? Created { get; set; } = null;
        public DateTime? Updated { get; set; } = null;
    }
}
