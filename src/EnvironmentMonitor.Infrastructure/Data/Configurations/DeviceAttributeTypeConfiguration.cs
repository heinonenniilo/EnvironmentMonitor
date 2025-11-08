using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvironmentMonitor.Domain.Entities;

namespace EnvironmentMonitor.Infrastructure.Data.Configurations
{
    public class DeviceAttributeTypeConfiguration : IEntityTypeConfiguration<DeviceAttributeType>
    {
        public void Configure(EntityTypeBuilder<DeviceAttributeType> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(d => d.Description)
                .HasMaxLength(512);

            builder.Property(d => d.Type)
                .IsRequired()
                .HasMaxLength(128);

            builder.HasIndex(d => d.Name).IsUnique();

            builder.HasMany(d => d.DeviceAttributes)
                .WithOne(s => s.Type)
                .HasForeignKey(s => s.TypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
