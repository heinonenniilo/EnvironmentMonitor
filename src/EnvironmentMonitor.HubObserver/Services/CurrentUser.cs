using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.HubObserver.Services
{
    public class CurrentUser : ICurrentUser
    {
        public string Email => "Function@Function.fi";

        public List<Claim> Claims => [new(ClaimTypes.Role.ToString(), GlobalRoles.Admin.ToString())];

        public List<string> Roles => [GlobalRoles.Admin.ToString()];
    }
}
