using System.Security.Claims;

namespace EnvironmentMonitor.Infrastructure.Identity;

public static class ClaimsConsolidationHelper
{
    /// <summary>
    /// Consolidates multiple claims of the same type into a single claim with semicolon-separated values.
    /// </summary>
    public static void ConsolidateClaims(ClaimsIdentity identity, string claimType)
    {
        var claims = identity.FindAll(claimType).ToList();
        if (claims.Count <= 1) return; // Nothing to consolidate

        // Remove individual claims
        foreach (var claim in claims)
        {
            identity.RemoveClaim(claim);
        }

        // Add consolidated claim with distinct values
        var consolidatedValue = string.Join(";", claims.Select(c => c.Value).Distinct());
        identity.AddClaim(new Claim(claimType, consolidatedValue));
    }

    /// <summary>
    /// Consolidates a list of claims by grouping them by type and joining values with semicolons.
    /// Returns a new list with consolidated claims.
    /// </summary>
    public static List<Claim> ConsolidateClaimsList(IEnumerable<Claim> claims)
    {
        var consolidatedClaims = new List<Claim>();

        var groupedClaims = claims.GroupBy(c => c.Type);

        foreach (var group in groupedClaims)
        {
            var distinctValues = group.Select(c => c.Value).Distinct().ToList();

            if (distinctValues.Count == 1)
            {
                // Single value, add as-is
                consolidatedClaims.Add(new Claim(group.Key, distinctValues[0]));
            }
            else
            {
                // Multiple values, consolidate with semicolon
                var consolidatedValue = string.Join(";", distinctValues);
                consolidatedClaims.Add(new Claim(group.Key, consolidatedValue));
            }
        }

        return consolidatedClaims;
    }
}
