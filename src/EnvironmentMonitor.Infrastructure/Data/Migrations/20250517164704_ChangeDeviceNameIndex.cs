using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDeviceNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Devices_Name",
                table: "Devices");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_Name_LocationId",
                table: "Devices",
                columns: new[] { "Name", "LocationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Devices_Name_LocationId",
                table: "Devices");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_Name",
                table: "Devices",
                column: "Name",
                unique: true);
        }
    }
}
