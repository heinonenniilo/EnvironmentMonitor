using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Domain.Interfaces
{
    public interface ICurrentUser
    {
        public string Email { get; }
        public List<Claim> Claims { get; }

        public List<string> Roles { get; }
    }
}
