using EnvironmentMonitor.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnvironmentMonitor.Infrastructure.Identity.Configurations;

public sealed class UserMfaMethodConfiguration : IEntityTypeConfiguration<UserMfaMethod>
{
    public void Configure(EntityTypeBuilder<UserMfaMethod> builder)
    {
        builder.HasKey(x => x.Id);

        builder
            .Property(x => x.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder
            .HasOne(x => x.User)
            .WithMany(x => x.MfaMethods)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(x => x.UpdatedById)
            .HasMaxLength(450);

        builder
            .HasOne(x => x.UpdatedBy)
            .WithMany()
            .HasForeignKey(x => x.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // A single row per user and method.
        builder
            .HasIndex(x => new { x.UserId, x.Method })
            .IsUnique();

        builder.Property(x => x.CreatedUtc).HasDefaultValueSql("GETUTCDATE()");
    }
}
