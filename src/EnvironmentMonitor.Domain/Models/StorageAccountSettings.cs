using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class StorageAccountSettings
    {
        public string ConnectionString { get; set; } = "";
        public required string ContainerName { get; set; }
        public required string AccountName { get; set; }
        public string AccountUri { get; set; } = "";
    }
}
