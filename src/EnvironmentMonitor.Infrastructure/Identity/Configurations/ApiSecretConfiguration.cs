using EnvironmentMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnvironmentMonitor.Infrastructure.Identity.Configurations
{
    public class ApiSecretConfiguration : IEntityTypeConfiguration<ApiSecret>
    {
        public void Configure(EntityTypeBuilder<ApiSecret> builder)
        {
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Id)
                .IsRequired()
                .HasMaxLength(40);

            builder.Property(x => x.Hash)
                .IsRequired();

            builder.Property(x => x.Created)
                .IsRequired();

            builder.Property(x => x.CreatedUtc)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.HasIndex(x => x.Id)
                .IsUnique();

            builder.HasMany(x => x.Claims)
                .WithOne(x => x.ApiSecret)
                .HasForeignKey(x => x.ApiSecretId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
