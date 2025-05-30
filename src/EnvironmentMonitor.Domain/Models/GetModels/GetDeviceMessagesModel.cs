using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models.GetModels
{
    public class GetDeviceMessagesModel : Paginated
    {
        public List<int>? DeviceIds { get; set; }
        public bool? IsDuplicate { get; set; }
        public bool? IsFirstMessage { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
