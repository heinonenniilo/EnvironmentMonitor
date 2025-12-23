using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceEmailTemplateIdentifie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Identifier",
                table: "DeviceEmailTemplates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEmailTemplates_Identifier",
                table: "DeviceEmailTemplates",
                column: "Identifier",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceEmailTemplates_Identifier",
                table: "DeviceEmailTemplates");

            migrationBuilder.DropColumn(
                name: "Identifier",
                table: "DeviceEmailTemplates");
        }
    }
}
