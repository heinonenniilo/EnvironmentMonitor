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
    public class DeviceTypeConfiguration : IEntityTypeConfiguration<DeviceType>
    {
        public void Configure(EntityTypeBuilder<DeviceType> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.HasMany(d => d.Devices).WithOne(x => x.Type).HasForeignKey(x => x.TypeId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(d => d.Name).IsUnique();
        }
    }
}
