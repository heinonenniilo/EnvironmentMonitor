using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DeviceMessageId",
                table: "Measurements",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeviceMessageId",
                table: "DeviceStatusChanges",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeviceMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeStampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceMessages_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_DeviceMessageId",
                table: "Measurements",
                column: "DeviceMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStatusChanges_DeviceMessageId",
                table: "DeviceStatusChanges",
                column: "DeviceMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMessages_DeviceId",
                table: "DeviceMessages",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceStatusChanges_DeviceMessages_DeviceMessageId",
                table: "DeviceStatusChanges",
                column: "DeviceMessageId",
                principalTable: "DeviceMessages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Measurements_DeviceMessages_DeviceMessageId",
                table: "Measurements",
                column: "DeviceMessageId",
                principalTable: "DeviceMessages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceStatusChanges_DeviceMessages_DeviceMessageId",
                table: "DeviceStatusChanges");

            migrationBuilder.DropForeignKey(
                name: "FK_Measurements_DeviceMessages_DeviceMessageId",
                table: "Measurements");

            migrationBuilder.DropTable(
                name: "DeviceMessages");

            migrationBuilder.DropIndex(
                name: "IX_Measurements_DeviceMessageId",
                table: "Measurements");

            migrationBuilder.DropIndex(
                name: "IX_DeviceStatusChanges_DeviceMessageId",
                table: "DeviceStatusChanges");

            migrationBuilder.DropColumn(
                name: "DeviceMessageId",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "DeviceMessageId",
                table: "DeviceStatusChanges");
        }
    }
}
