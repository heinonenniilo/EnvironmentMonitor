using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using EnvironmentMonitor.Domain.Entities;
using Attachment = EnvironmentMonitor.Domain.Entities.Attachment;

namespace EnvironmentMonitor.Infrastructure.Data.Configurations
{
    public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.HasKey(d => d.Id);
            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Extension).IsRequired();
            builder.Property(x => x.Path).IsRequired();
            builder.Property(x => x.FullPath);
            builder.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()").IsRequired();
            builder.HasMany(x => x.DevicesDefaultImage).WithOne(x => x.DefaultImage).HasForeignKey(x => x.DefaultImageId).IsRequired(false);
        }
    }
}
