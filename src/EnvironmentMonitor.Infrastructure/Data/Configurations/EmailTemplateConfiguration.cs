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

            builder.HasData(Enum.GetValues(typeof(EmailTemplateTypes))
                .Cast<EmailTemplateTypes>()
                .Select(e => new EmailTemplate
                {
                    Id = (int)e,
                    Name = GetName(e),
                    Title = GetDefaultTitle(e),
                    Message = GetDefaultMessage(e)
                })
                .ToList());
        }

        private string GetName(EmailTemplateTypes type)
        {
            return type.GetDescription();
        }

        private string? GetDefaultTitle(EmailTemplateTypes type)
        {
            return type switch
            {
                EmailTemplateTypes.ConfirmUserEmail => "Confirm Your Email Address",
                EmailTemplateTypes.UserPasswordReset => "Reset Your Password",
                _ => null
            };
        }

        private string? GetDefaultMessage(EmailTemplateTypes type)
        {
            return type switch
            {
                EmailTemplateTypes.ConfirmUserEmail => "Please confirm your email address by clicking the link below:\n\n{ConfirmationLink}\n\nIf you did not create an account, please ignore this email.",
                EmailTemplateTypes.UserPasswordReset => "You have requested to reset your password. Please click the link below to set a new password:\n\n{ResetLink}\n\nIf you did not request a password reset, please ignore this email.",
                _ => null
            };
        }
    }
}

