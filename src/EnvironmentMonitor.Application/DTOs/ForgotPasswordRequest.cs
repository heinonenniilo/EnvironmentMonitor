using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Application.DTOs
{
    public class ForgotPasswordRequest
    {
        public required string Email { get; set; }
    }
}
