using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class EmailSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string SenderAddress { get; set; } = string.Empty;
        public List<string> RecipientAddresses { get; set; } = new List<string>();
        public string EmailTitlePrefix { get; set; } = string.Empty;
    }
}
