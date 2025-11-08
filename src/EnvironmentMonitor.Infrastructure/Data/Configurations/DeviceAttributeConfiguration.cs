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
    public class DeviceAttributeConfiguration : IEntityTypeConfiguration<DeviceAttribute>
    {
        public void Configure(EntityTypeBuilder<DeviceAttribute> builder)
        {
            builder.HasKey(d => d.Id);
            
            builder.Property(x => x.DeviceId).IsRequired();
            builder.Property(x => x.TypeId).IsRequired();
            builder.Property(x => x.Value).HasMaxLength(1024);
            builder.Property(x => x.TimeStamp).IsRequired();
            builder.Property(x => x.TimeStampUtc).IsRequired();
            builder.Property(x => x.CreatedUtc).IsRequired();
            builder.Property(x => x.Created).IsRequired();

            // Unique index on DeviceId and TypeId combination
            builder.HasIndex(d => new { d.DeviceId, d.TypeId }).IsUnique();
            
            // Additional index for querying by timestamp
            builder.HasIndex(d => new { d.TimeStamp, d.DeviceId });
        }
    }
}
