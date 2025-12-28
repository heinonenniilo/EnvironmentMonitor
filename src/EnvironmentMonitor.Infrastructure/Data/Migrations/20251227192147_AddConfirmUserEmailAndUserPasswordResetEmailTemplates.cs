using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConfirmUserEmailAndUserPasswordResetEmailTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "Id", "Created", "Message", "Name", "Title", "Updated" },
                values: new object[,]
                {
                    { 2, null, "Please confirm your email address by clicking the link below:\n\n{ConfirmationLink}\n\nIf you did not create an account, please ignore this email.", "Confirm Email", "Confirm Your Email Address", null },
                    { 3, null, "You have requested to reset your password. Please click the link below to set a new password:\n\n{ResetLink}\n\nIf you did not request a password reset, please ignore this email.", "User Password Reset", "Reset Your Password", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
