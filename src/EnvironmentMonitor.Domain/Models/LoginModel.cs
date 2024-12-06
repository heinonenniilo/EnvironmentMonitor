using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class LoginModel
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public bool Persistent { get; set; }
    }

    public class ExternalLoginModel
    {
        public string LoginProvider { get; set; }

        public string ProviderKey { get; set; }
        public required bool Persistent { get; set; }
        public string UserName { get; set; }
    }
}
