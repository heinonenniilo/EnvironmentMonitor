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
    public class LocationSensorConfiguration : IEntityTypeConfiguration<LocationSensor>
    {
        public void Configure(EntityTypeBuilder<LocationSensor> builder)
        {
            builder.HasKey(d => new { d.LocationId, d.SensorId, d.DeviceId });
            builder.Property(x => x.Name).IsRequired();
            /*
            builder.HasOne(x => x.Device).WithMany(x => x.LocationSensors).HasForeignKey(x => new { x.DeviceId, x.LocationId }).HasPrincipalKey(x => new { x.Id, x.LocationId }).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Sensor).WithMany(x => x.LocationSensors).HasForeignKey(x => new { x.SensorId, x.DeviceId }).HasPrincipalKey(x => new { x.Id, x.DeviceId }).OnDelete(DeleteBehavior.Restrict);
            */
        }
    }
}
