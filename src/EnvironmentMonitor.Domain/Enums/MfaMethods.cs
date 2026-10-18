using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Enums
{
    public enum MfaMethods
    {
        [Description("Email")]
        Email = 0,
        [Description("Authenticator app")]
        Authenticator = 1
    }
}
