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
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(512);

            builder.Property(x => x.DeviceIdentifier).IsRequired();
            builder.HasIndex(x => x.DeviceIdentifier).IsUnique();

            builder.HasIndex(d => new { d.Name, d.LocationId }).IsUnique();

            builder.Property(x => x.Visible).HasDefaultValue(true);

            builder.Property(x => x.Identifier).HasDefaultValueSql("NEWID()");
            builder.Property(x => x.Identifier).IsRequired();
            builder.HasIndex(x => x.Identifier).IsUnique();

            builder.HasMany(d => d.Sensors)
                .WithOne(s => s.Device)
                .IsRequired(false)
                .HasForeignKey(s => s.DeviceId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.DeviceMessages).WithOne(x => x.Device).IsRequired();
            builder.HasMany(d => d.StatusChanges).WithOne(x => x.Device).IsRequired();
            builder.HasMany(x => x.Events).WithOne(x => x.Device).HasForeignKey(x => x.DeviceId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.LocationSensors).WithOne(x => x.Device).HasForeignKey(x => new { x.DeviceId, x.LocationId }).HasPrincipalKey(x => new { x.Id, x.LocationId }).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
