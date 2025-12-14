using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Enums;
using EnvironmentMonitor.Domain.Extensions;

namespace EnvironmentMonitor.Infrastructure.Data.Configurations
{
    public class DeviceEmailTemplateConfiguration : IEntityTypeConfiguration<DeviceEmailTemplate>
    {
        public void Configure(EntityTypeBuilder<DeviceEmailTemplate> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.Title)
                .IsRequired(false)
                .HasMaxLength(512);

            builder.Property(x => x.Created);

            builder.Property(x => x.Updated)
                .IsRequired(false);

            builder.Property(x => x.Message)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.CreatedUtc).HasDefaultValueSql("GETUTCDATE()");

            builder.HasData(Enum.GetValues(typeof(DeviceEmailTemplateTypes))
                .Cast<DeviceEmailTemplateTypes>()
                .Select(e => new DeviceEmailTemplate
                {
                    Id = (int)e
                })
                .ToList());
        }
    }
}
