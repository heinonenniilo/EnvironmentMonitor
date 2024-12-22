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
    public class DeviceEventonfiguration : IEntityTypeConfiguration<DeviceEvent>
    {
        public void Configure(EntityTypeBuilder<DeviceEvent> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(x => x.DeviceId).IsRequired();
            builder.Property(x => x.TypeId).IsRequired();
            builder.Property(d => d.Message).HasMaxLength(1024);
            builder.Property(x => x.TimeStamp).IsRequired();
            builder.Property(x => x.TimeStampUtc).IsRequired();
            builder.HasIndex(d => new { d.TimeStamp, d.DeviceId });
        }
    }
}
