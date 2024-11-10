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
            // builder.ToTable("Measurements");
            builder.HasKey(m => m.Id);

            // Properties
            builder.Property(m => m.Timestamp)
                .IsRequired();

            builder.Property(m => m.Value)
                .IsRequired();
        }
    }
}
