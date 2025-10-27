using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class StoreSecretReturnModel
    {
        public required string SecretName { get; set; }
        public required Uri Identifier { get; set; }
    }
}
