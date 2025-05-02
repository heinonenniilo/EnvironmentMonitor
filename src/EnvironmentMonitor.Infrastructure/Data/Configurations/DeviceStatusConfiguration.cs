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
    public class DeviceStatusConfiguration : IEntityTypeConfiguration<DeviceStatus>
    {
        public void Configure(EntityTypeBuilder<DeviceStatus> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Message)
                .HasColumnType("nvarchar(max)");
            builder.HasIndex(x => new { x.TimeStamp, x.DeviceId, x.Status });
        }
    }
}
