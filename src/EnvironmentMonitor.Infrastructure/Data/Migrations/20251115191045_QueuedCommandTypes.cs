using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class QueuedCommandTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceQueuedCommandTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceQueuedCommandTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DeviceQueuedCommandTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 0, "Send stored device attributes", "SendDeviceAttributes" },
                    { 1, "Set motion control status", "SetMotionControlStatus" },
                    { 2, "Set motion control delay", "SetMotionControlOnDelay" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceQueuedCommands_Type",
                table: "DeviceQueuedCommands",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceQueuedCommandTypes_Name",
                table: "DeviceQueuedCommandTypes",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceQueuedCommands_DeviceQueuedCommandTypes_Type",
                table: "DeviceQueuedCommands",
                column: "Type",
                principalTable: "DeviceQueuedCommandTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceQueuedCommands_DeviceQueuedCommandTypes_Type",
                table: "DeviceQueuedCommands");

            migrationBuilder.DropTable(
                name: "DeviceQueuedCommandTypes");

            migrationBuilder.DropIndex(
                name: "IX_DeviceQueuedCommands_Type",
                table: "DeviceQueuedCommands");
        }
    }
}
