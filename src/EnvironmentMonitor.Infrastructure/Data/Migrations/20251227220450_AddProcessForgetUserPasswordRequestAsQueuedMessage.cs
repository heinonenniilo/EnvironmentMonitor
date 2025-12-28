using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessForgetUserPasswordRequestAsQueuedMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DeviceQueuedCommandTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 4, "Process forget user password request", "ProcessForgetUserPasswordRequest" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DeviceQueuedCommandTypes",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
