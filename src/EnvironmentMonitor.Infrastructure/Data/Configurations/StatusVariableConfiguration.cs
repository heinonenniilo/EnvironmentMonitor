using EnvironmentMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnvironmentMonitor.Infrastructure.Data.Configurations
{
    public class StatusVariableConfiguration : IEntityTypeConfiguration<StatusVariable>
    {
        public void Configure(EntityTypeBuilder<StatusVariable> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Key)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.Value)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.CreatedUtc)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.UpdatedUtc)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint on Key to ensure only one value per key
            builder.HasIndex(x => x.Key)
                .IsUnique();
        }
    }
}
