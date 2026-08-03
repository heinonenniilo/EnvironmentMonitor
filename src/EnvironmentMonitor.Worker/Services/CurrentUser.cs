using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Interfaces;
using System.Security.Claims;

namespace EnvironmentMonitor.Worker.Services
{
    public class CurrentUser : ICurrentUser
    {
        public string Email => "Worker@Worker.service";

        public List<Claim> Claims => [new(ClaimTypes.Role.ToString(), GlobalRoles.Admin.ToString())];

        public List<string> Roles => [GlobalRoles.Admin.ToString()];
    }
}
