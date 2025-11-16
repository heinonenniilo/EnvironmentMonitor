using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class QueueMessageInfo
    {
        public required string MessageId { get; set; }
        public string MessageText { get; set; } = string.Empty;
        public string PopReceipt { get; set; } = string.Empty;
        public DateTime ScheludedToExecuteUtc { get; set; }
        public DateTime InsertedOnUtc { get; set; }
    }
}
