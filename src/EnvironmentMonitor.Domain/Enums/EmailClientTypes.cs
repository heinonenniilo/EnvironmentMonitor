using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Enums
{
    public enum EmailClientTypes
    {
        AzureCommunicationService = 0,
        Smtp = 1,
        MailGun = 2
    }
}
