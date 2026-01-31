using EnvironmentMonitor.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EnvironmentMonitor.HubObserver.Functions
{
    public class FmiMeasurementSync
    {
        private readonly ILogger<FmiMeasurementSync> _logger;
        private readonly IFmiMeasurementService _fmiMeasurementService;

        public FmiMeasurementSync(
            ILogger<FmiMeasurementSync> logger,
            IFmiMeasurementService fmiMeasurementService)
        {
            _logger = logger;
            _fmiMeasurementService = fmiMeasurementService;
        }

        [Function(nameof(FmiMeasurementSync))]
        [FixedDelayRetry(3, "00:00:30")]
        public async Task Run(
            [TimerTrigger("%FmiMeasurementSyncSchedule%")] TimerInfo timerInfo,
            FunctionContext context)
        {
            _logger.LogInformation($"FMI measurement sync timer trigger function started at: {DateTime.UtcNow}");

            try
            {
                await _fmiMeasurementService.PerformSync();
                _logger.LogInformation($"FMI measurement sync completed successfully at: {DateTime.UtcNow}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during FMI measurement sync");
                throw;
            }
        }
    }
}
