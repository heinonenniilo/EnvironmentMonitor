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
    public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
    {
        public void Configure(EntityTypeBuilder<EmailTemplate> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.Identifier)
                .IsRequired()
                .HasDefaultValueSql("NEWID()");

            builder.HasIndex(x => x.Identifier)
                .IsUnique();

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

            builder.Property(x => x.Name).HasMaxLength(512);

            builder.HasIndex(x => x.Name).IsUnique();

            builder.HasData(Enum.GetValues(typeof(DeviceEmailTemplateTypes))
                .Cast<DeviceEmailTemplateTypes>()
                .Select(e => new EmailTemplate
                {
                    Id = (int)e,
                    Name = GetName(e),
                })
                .ToList());
        }

        private string GetName(DeviceEmailTemplateTypes type)
        {
            return type.GetDescription();
        }
    }
}

