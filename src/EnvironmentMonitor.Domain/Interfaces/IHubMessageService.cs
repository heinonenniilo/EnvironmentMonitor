﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface IHubMessageService
    {
        public Task SendMessageToDevice(string deviceIdentifier, string message);
    }
}
