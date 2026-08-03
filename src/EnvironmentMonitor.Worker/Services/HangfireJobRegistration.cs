using EnvironmentMonitor.Domain.Models;
using EnvironmentMonitor.Worker.Jobs;
using Hangfire;

namespace EnvironmentMonitor.Worker.Services
{
    /// <summary>
    /// Service responsible for registering all Hangfire recurring jobs.
    /// Job schedules are configured in HangfireJobSchedules class.
    /// </summary>
    public static class HangfireJobRegistration
    {
        // Hangfire job identifiers
        private const string FmiMeasurementSyncJobId = "fmi-measurement-sync";

        /// <summary>
        /// Registers all recurring Hangfire jobs with their schedules.
        /// Reads schedules from HangfireJobs section in configuration.
        /// </summary>
        /// <param name="configuration">Application configuration to read schedules from</param>
        /// <param name="recurringJobManager">Hangfire recurring job manager from DI</param>
        public static void RegisterJobs(IConfiguration configuration, IRecurringJobManager recurringJobManager)
        {
            // Bind job schedules from configuration
            var schedules = new HangfireJobSchedules();
            configuration.GetSection("HangfireJobs").Bind(schedules);

            recurringJobManager.AddOrUpdate<FmiMeasurementSyncJob>(
                FmiMeasurementSyncJobId,
                job => job.Execute(),
                schedules.FmiSyncSchedule);
        }
    }
}
