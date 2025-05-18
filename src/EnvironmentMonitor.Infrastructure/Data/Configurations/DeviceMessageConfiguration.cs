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
    public class DeviceMessageConfiguration : IEntityTypeConfiguration<DeviceMessage>
    {
        public void Configure(EntityTypeBuilder<DeviceMessage> builder)
        {
            builder.HasKey(d => d.Id);
            builder.HasMany(x => x.Measurements).WithOne(x => x.DeviceMessage).IsRequired(false);
            builder.Property(x => x.Identifier).HasMaxLength(128);
            builder.HasIndex(x => new { x.DeviceId, x.TimeStamp });
            builder.HasIndex(x => x.TimeStamp);
        }
    }
}
