using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Enums
{
    public enum EmailTemplateTypes
    {
        [Description("Device connection lost")]
        ConnectionLost = 0,
        [Description("Device connection restored")]
        ConnectionOk = 1
    }
}
