using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class ChangePasswordModel
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}
