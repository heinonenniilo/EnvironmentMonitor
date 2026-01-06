using Microsoft.AspNetCore.Identity;
using System;

namespace EnvironmentMonitor.Infrastructure.Identity;

public class ApplicationUserClaim : IdentityUserClaim<string>
{
    public DateTime? Updated { get; set; }
    public DateTime? UpdatedUtc { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public string? UpdatedById { get; set; }
    public ApplicationUser? UpdatedBy { get; set; }
}
