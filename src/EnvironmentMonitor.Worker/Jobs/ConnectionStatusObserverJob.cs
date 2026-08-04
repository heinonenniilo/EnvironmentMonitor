using EnvironmentMonitor.Application.Interfaces;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Domain.Models;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnvironmentMonitor.Worker.Jobs
{
    public class ConnectionStatusObserverJob
    {
        private ILogger<ConnectionStatusObserverJob> _logger;
        private IDeviceService _deviceService;
        private IDateService _dateService;

        public ConnectionStatusObserverJob(ILogger<ConnectionStatusObserverJob> logger, 
            IDeviceService deviceService
            ,IDateService dateService)
        {
            _logger = logger;
            _deviceService = deviceService;
            _dateService = dateService;
        }


        [DisableConcurrentExecution(timeoutInSeconds: 300)]
        public async Task Execute()
        {
            _logger.LogInformation($"Starting connection status observer job at: {_dateService.CurrentTime()}");
            var devices = await _deviceService.GetDeviceInfos(false, null, false);
            foreach (var device in devices)
            {
                _logger.LogInformation($"Checking connection status for device: {device.Device.Name} ({device.Device.Identifier})");
                await _deviceService.SetStatus(new SetDeviceStatusModel()
                {
                    Idenfifier = device.Device.Identifier,
                    Message = $"Recurring check"
                }, true);
            }
            _logger.LogInformation($"Connection status observer job completed at: {_dateService.CurrentTime()}");
        }
    }
}
