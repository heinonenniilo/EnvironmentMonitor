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
    public class MeasurementConfiguration : IEntityTypeConfiguration<Measurement>
    {
        public void Configure(EntityTypeBuilder<Measurement> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Timestamp).IsRequired();
            builder.Property(m => m.CreatedAtUtc).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.Property(m => m.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");

            builder.Property(m => m.Value)
                .IsRequired();
        }
    }
}
