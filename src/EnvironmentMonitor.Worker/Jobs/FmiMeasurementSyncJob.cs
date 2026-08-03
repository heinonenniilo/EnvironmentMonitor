using EnvironmentMonitor.Application.Interfaces;
using Hangfire;

namespace EnvironmentMonitor.Worker.Jobs
{
    /// <summary>
    /// Hangfire job for syncing FMI weather measurement data.
    /// DisableConcurrentExecution prevents multiple instances from running simultaneously.
    /// </summary>
    
    public class FmiMeasurementSyncJob
    {
        private readonly IFmiMeasurementService _fmiMeasurementService;
        private readonly ILogger<FmiMeasurementSyncJob> _logger;

        public FmiMeasurementSyncJob(
            IFmiMeasurementService fmiMeasurementService,
            ILogger<FmiMeasurementSyncJob> logger)
        {
            _fmiMeasurementService = fmiMeasurementService;
            _logger = logger;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 300)]
        public async Task Execute()
        {
            _logger.LogInformation("FMI measurement sync job started at: {Time}", DateTimeOffset.UtcNow);

            try
            {
                await _fmiMeasurementService.SyncData();
                _logger.LogInformation("FMI measurement sync job completed successfully at: {Time}", DateTimeOffset.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during FMI measurement sync job");
                throw; // Re-throw to let Hangfire handle retry logic
            }
        }
    }
}
