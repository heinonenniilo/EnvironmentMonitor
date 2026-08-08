using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace EnvironmentMonitor.Infrastructure.Identity;

public class ConsolidatedClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationUserRole>
{
    public ConsolidatedClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationUserRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // Consolidate entity claims (Sensor, Device, Location)
        ClaimsConsolidationHelper.ConsolidateClaims(identity, "Sensor");
        ClaimsConsolidationHelper.ConsolidateClaims(identity, "Device");
        ClaimsConsolidationHelper.ConsolidateClaims(identity, "Location");

        return identity;
    }
}
