using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceQueuedCommandOriginalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OriginalId",
                table: "DeviceQueuedCommands",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceQueuedCommands_OriginalId",
                table: "DeviceQueuedCommands",
                column: "OriginalId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceQueuedCommands_DeviceQueuedCommands_OriginalId",
                table: "DeviceQueuedCommands",
                column: "OriginalId",
                principalTable: "DeviceQueuedCommands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceQueuedCommands_DeviceQueuedCommands_OriginalId",
                table: "DeviceQueuedCommands");

            migrationBuilder.DropIndex(
                name: "IX_DeviceQueuedCommands_OriginalId",
                table: "DeviceQueuedCommands");

            migrationBuilder.DropColumn(
                name: "OriginalId",
                table: "DeviceQueuedCommands");
        }
    }
}
