using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Enums
{
    public enum CommunicationChannels
    {
        [Description("IoT Hub")]
        IotHub = 0,
        [Description("Rest API")]
        RestApi = 1
    }
}
