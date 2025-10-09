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
            builder.Property(x => x.Name).HasMaxLength(512).IsRequired();
            builder.Property(x => x.OriginalName).HasMaxLength(512);
            builder.Property(x => x.Extension).IsRequired();
            builder.Property(x => x.Path).HasMaxLength(1024).IsRequired();
            builder.Property(x => x.FullPath).HasMaxLength(1024);
            builder.HasIndex(x => x.Name).IsUnique();           
        }
    }
}
