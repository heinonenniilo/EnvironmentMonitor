using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSendEmailAsQueuedCommandType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DeviceQueuedCommandTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 3, "Send email about a device", "SendDeviceEmail" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DeviceQueuedCommandTypes",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
