using EnvironmentMonitor.Domain.Enums;
using Hangfire.Dashboard;

namespace EnvironmentMonitor.WebApi.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                return httpContext.User.IsInRole(GlobalRoles.Admin.ToString());
            }

            return false;
        }
    }
}
