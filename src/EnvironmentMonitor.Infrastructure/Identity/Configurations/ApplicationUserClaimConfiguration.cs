using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnvironmentMonitor.Infrastructure.Identity.Configurations;

public sealed class ApplicationUserClaimConfiguration : IEntityTypeConfiguration<ApplicationUserClaim>
{
    public void Configure(EntityTypeBuilder<ApplicationUserClaim> builder)
    {
        // Set reasonable max lengths for ClaimType and ClaimValue as there is an index.
        builder
            .Property(uc => uc.ClaimType)
            .HasMaxLength(125);

        builder
            .Property(uc => uc.ClaimValue)
            .HasMaxLength(125);

        builder
            .HasIndex(uc => new { uc.ClaimType, uc.ClaimValue, uc.UserId })
            .IsUnique();

        builder
            .HasOne(uc => uc.UpdatedBy)
            .WithMany()
            .HasForeignKey(uc => uc.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(uc => uc.UpdatedById)
            .HasMaxLength(450);

        builder
            .Property(uc => uc.CreatedUtc)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
