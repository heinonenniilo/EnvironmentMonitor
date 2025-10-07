using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models.AddModels
{
    public class AddDeviceAttachmentModel : AddAttachmentModel<Guid>
    {
        public required bool IsDeviceImage { get; set; }
    }
}
