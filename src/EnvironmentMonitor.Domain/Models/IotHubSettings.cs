using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class IotHubSettings
    {
        public string IotHubDomain { get; set; }     
        public string ConnectionString { get; set; } // TODO NOT USED YET
        public string? TenantId { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
    }
}
