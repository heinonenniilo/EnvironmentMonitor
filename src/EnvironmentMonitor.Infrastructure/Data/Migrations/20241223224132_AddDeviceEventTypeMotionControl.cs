using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceEventTypeMotionControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DeviceEventTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 2, "Set motion control status", "SetMotionControlStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DeviceEventTypes",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
