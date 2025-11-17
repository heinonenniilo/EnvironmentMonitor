using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceQueuedMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceQueuedCommands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    Scheduled = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(MAX)", maxLength: 256, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceQueuedCommands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceQueuedCommands_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceQueuedCommands_DeviceId_ExecutedAtUtc",
                table: "DeviceQueuedCommands",
                columns: new[] { "DeviceId", "ExecutedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceQueuedCommands_DeviceId_ScheduledUtc",
                table: "DeviceQueuedCommands",
                columns: new[] { "DeviceId", "ScheduledUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceQueuedCommands_MessageId",
                table: "DeviceQueuedCommands",
                column: "MessageId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceQueuedCommands");
        }
    }
}
