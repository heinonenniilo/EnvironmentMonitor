using EnvironmentMonitor.Domain.Interfaces;
using System.Security.Claims;

namespace EnvironmentMonitor.WebApi.Services
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ?? "";
        public List<Claim> Claims => _httpContextAccessor.HttpContext?.User?.Claims?.ToList() ?? [];
        public List<string> Roles => _httpContextAccessor.HttpContext?.User?.Claims?.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList() ?? [];
    }
}
