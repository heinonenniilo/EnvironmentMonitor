using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Entities
{
    public class DeviceQueuedCommand : TrackedEntity
    {
        public int Id { get; set; }
        public required string MessageId { get; set; }
        public string PopReceipt { get; set; }
        public int DeviceId { get; set; }
        public Device Device { get; set; }
        public DateTime Scheduled { get; set; }
        public DateTime ScheduledUtc { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public DateTime? ExecutedAtUtc { get; set; }
        public required int Type { get; set; }
        public DeviceQueuedCommandType CommandType { get; set; }
        public required string Message { get; set; }
        public bool IsRemoved { get; set; }

        public int? OriginalId { get; set; }
        public DeviceQueuedCommand? OriginalCommand { get; set; }
    }
}
