namespace EnvironmentMonitor.Domain.Models
{
    /// <summary>
    /// Configuration for all Hangfire recurring jobs.
    /// Key matches the configuration key in appsettings.json under "HangfireJobs".
    /// </summary>
    public class HangfireJobSchedules
    {
        public string FmiSyncSchedule { get; set; } = "*/5 * * * *";
        public string ConnectionStatusObserverSchedule { get; set; } = "*/5 * * * *";
    }
}
