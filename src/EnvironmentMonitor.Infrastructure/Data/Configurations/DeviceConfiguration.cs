using EnvironmentMonitor.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Data.Configurations
{
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.ToTable("Devices");
            // Primary Key
            builder.HasKey(d => d.Id);
            // Properties
            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(512);
            builder.HasIndex(d => d.Name).IsUnique();
            // Relationships
            builder.HasMany(d => d.Sensors)
                .WithOne(s => s.Device)
                .HasForeignKey(s => s.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
