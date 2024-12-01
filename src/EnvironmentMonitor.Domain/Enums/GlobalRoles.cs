using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Enums
{
    public enum GlobalRoles
    {
        [Description("Can view everything")]
        Viewer = 0,
        [Description("Admin access")]
        Admin = 1,
    }
}
