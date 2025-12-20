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
    public class DeviceContactConfiguration : IEntityTypeConfiguration<DeviceContact>
    {
        public void Configure(EntityTypeBuilder<DeviceContact> builder)
        {
            builder.HasKey(d => d.Id);
            
            builder.Property(x => x.Identifier)
                .IsRequired()
                .HasDefaultValueSql("NEWID()");
            
            builder.Property(x => x.DeviceId).IsRequired();
            
            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(1024);
            
            builder.Property(x => x.CreatedUtc).IsRequired();
            builder.Property(x => x.Created).IsRequired();
            
            builder.HasIndex(x => x.Identifier).IsUnique();
            
            builder.HasIndex(x => new { x.DeviceId, x.Email }).IsUnique();
        }
    }
}
