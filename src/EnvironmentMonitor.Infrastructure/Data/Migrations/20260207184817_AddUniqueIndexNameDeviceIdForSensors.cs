using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexNameDeviceIdForSensors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sensors_DeviceId_Name",
                table: "Sensors",
                columns: new[] { "DeviceId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sensors_DeviceId_Name",
                table: "Sensors");
        }
    }
}
