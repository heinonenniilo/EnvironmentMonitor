using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSendAttributesDeviceEventType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DeviceEventTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 4, "Send stored attributes", "SendAttributes" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DeviceEventTypes",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
