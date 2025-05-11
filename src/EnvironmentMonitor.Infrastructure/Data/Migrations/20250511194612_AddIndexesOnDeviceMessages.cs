using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesOnDeviceMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceMessages_DeviceId",
                table: "DeviceMessages");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMessages_DeviceId_TimeStamp",
                table: "DeviceMessages",
                columns: new[] { "DeviceId", "TimeStamp" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMessages_TimeStamp",
                table: "DeviceMessages",
                column: "TimeStamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceMessages_DeviceId_TimeStamp",
                table: "DeviceMessages");

            migrationBuilder.DropIndex(
                name: "IX_DeviceMessages_TimeStamp",
                table: "DeviceMessages");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMessages_DeviceId",
                table: "DeviceMessages",
                column: "DeviceId");
        }
    }
}
