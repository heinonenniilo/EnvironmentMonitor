using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Models
{
    public class RegisterUserModel
    {
        public required string Email { get; set; }
        public required string Password { get; set; }

        public required string ConfirmPassword { get; set; }
        public string? BaseUrl { get; set; }
    }
}
