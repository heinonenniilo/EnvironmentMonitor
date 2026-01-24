using EnvironmentMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnvironmentMonitor.Infrastructure.Identity.Configurations
{
    public class SecretClaimConfiguration : IEntityTypeConfiguration<SecretClaim>
    {
        public void Configure(EntityTypeBuilder<SecretClaim> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Value)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.ApiSecretId)
                .IsRequired()
                .HasMaxLength(40);

            builder.HasIndex(x => new { x.ApiSecretId, x.Type, x.Value });
        }
    }
}
