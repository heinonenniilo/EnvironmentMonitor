using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailTemplatesName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "DeviceEmailTemplates",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "DeviceEmailTemplates",
                keyColumn: "Id",
                keyValue: 0,
                column: "Name",
                value: "Device connection lost");

            migrationBuilder.UpdateData(
                table: "DeviceEmailTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Device connection restored");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEmailTemplates_Name",
                table: "DeviceEmailTemplates",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceEmailTemplates_Name",
                table: "DeviceEmailTemplates");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "DeviceEmailTemplates");
        }
    }
}
