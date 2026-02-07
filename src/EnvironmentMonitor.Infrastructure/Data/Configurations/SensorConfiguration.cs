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
    public class SensorConfiguration : IEntityTypeConfiguration<Sensor>
    {
        public void Configure(EntityTypeBuilder<Sensor> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(512);
            builder.Property(s => s.TypeId);
            builder.HasIndex(x => new { x.DeviceId, x.SensorId }).IsUnique();
            builder.Property(x => x.Identifier).HasDefaultValueSql("newid()").IsRequired();
            builder.HasIndex(x => x.Identifier).IsUnique();

            builder.HasIndex(x => new { x.DeviceId, x.Name }).IsUnique();

            builder.Property(x => x.Active).HasDefaultValue(true);
            builder.HasMany(s => s.Measurements)
                .WithOne(m => m.Sensor)
                .HasForeignKey(m => m.SensorId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(s => s.LocationSensors).WithOne(x => x.Sensor).HasForeignKey(x => new { x.SensorId, x.DeviceId }).HasPrincipalKey(x => new { x.Id, x.DeviceId }).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(s => s.PublicSensors)
                .WithOne(ps => ps.Sensor)
                .HasForeignKey(ps => ps.SensorId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.VirtualSensorRows).WithOne(x => x.VirtualSensor).HasForeignKey(x => x.VirtualSensorId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(s => s.VirtualSensorRowValues).WithOne(x => x.ValueSensor).HasForeignKey(x => x.ValueSensorId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        }
    }
}