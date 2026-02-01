using EnvironmentMonitor.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using EnvironmentMonitor.Domain.Models;

namespace EnvironmentMonitor.HubObserver.Functions
{
    public class ConnectionStatusObserver
    {
        private ILogger<ConnectionStatusObserver> _logger;
        private IDeviceService _deviceService;
        public ConnectionStatusObserver(ILogger<ConnectionStatusObserver> logger, IDeviceService deviceService)
        {
            _logger = logger;
            _deviceService = deviceService;
        }

        [Function(nameof(ConnectionStatusObserver))]
        [FixedDelayRetry(3, "00:00:05")]
        public async Task Run([TimerTrigger("%ConnectionStatusSchedule%")] TimerInfo timerInfo,
            FunctionContext context)
        {
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
        }
    }
}
