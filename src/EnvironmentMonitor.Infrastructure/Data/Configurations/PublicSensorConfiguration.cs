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
    public class PublicSensorConfiguration : IEntityTypeConfiguration<PublicSensor>
    {
        public void Configure(EntityTypeBuilder<PublicSensor> builder)
        {
            builder.HasKey(ps => ps.Id);
            
            builder.Property(ps => ps.Name)
                .IsRequired()
                .HasMaxLength(512);
            
            builder.Property(ps => ps.TypeId);
            
            builder.HasIndex(ps => ps.SensorId)
                .IsUnique();
        }
    }
}