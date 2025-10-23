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
    public class VirtualSensorRowConfiguration : IEntityTypeConfiguration<VirtualSensorRow>
    {
        public void Configure(EntityTypeBuilder<VirtualSensorRow> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(x => x.CreatedUtc).HasDefaultValueSql("GETUTCDATE()");
            builder.HasIndex(x => new { x.VirtualSensorId, x.ValueSensorId }).IsUnique();
        }
    }
}