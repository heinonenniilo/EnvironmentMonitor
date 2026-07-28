using System;

namespace EnvironmentMonitor.Domain.Entities
{
    /// <summary>
    /// Stores application-level status variables and configuration values.
    /// Used for tracking state like last synced message ID, etc.
    /// </summary>
    public class StatusVariable
    {
        public int Id { get; set; }
        public required string Key { get; set; }
        public required string Value { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
    }
}
