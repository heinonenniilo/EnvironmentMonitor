using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class KeyVaultSettings
    {
        public string VaultUri { get; set; } = string.Empty;
        public bool Base64EncodeSecrets { get; set; } = false;
        public string? TenantId { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
    }
}
