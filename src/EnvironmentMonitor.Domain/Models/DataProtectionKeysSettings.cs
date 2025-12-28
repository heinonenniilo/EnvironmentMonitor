using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class DataProtectionKeysSettings
    {
        public bool StoreInDatabase { get; set; }
        public bool EncryptWithKeyVault { get; set; }
        public string KeyVaultKeyIdentifier { get; set; } = string.Empty;
    }
}
