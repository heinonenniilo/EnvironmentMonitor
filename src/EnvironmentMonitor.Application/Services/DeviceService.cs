using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IHubMessageService _messageService;
        private readonly ILogger<DeviceService> _logger;
        private readonly IUserService _userService;


        public DeviceService(IHubMessageService messageService, ILogger<DeviceService> logger, IUserService userService)
        {
            _messageService = messageService;
            _logger = logger;
        }
        public async Task Reboot(string deviceIdentifier)
        {
            if (!_userService.IsAdmin)
            {
                throw new UnauthorizedAccessException();
            }
            _logger.LogInformation($"Trying to reboot device with identifier '{deviceIdentifier}'");
            await _messageService.SendMessageToDevice(deviceIdentifier, "REBOOT");
        }
    }
}
