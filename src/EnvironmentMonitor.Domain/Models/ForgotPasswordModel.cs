using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class ForgotPasswordModel
    {
        public required string Email { get; set; }
        public string? BaseUrl { get; set; }
        public bool Enqueue { get; set; } = false;
    }
}
