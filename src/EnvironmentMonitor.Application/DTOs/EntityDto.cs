using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class EntityDto
    {
        public Guid Identifier { get; set; }
        public required string Name { get; set; }
        public string? DisplayName { get; set; }
    }
}
