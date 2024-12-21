using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeviceEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceEventTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceEventTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeStampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    TypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceEvents_DeviceEventTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DeviceEventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceEvents_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DeviceEventTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 0, "Reboot command", "RebootCommand" },
                    { 1, "First message after boot / online since", "Online" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_DeviceId",
                table: "DeviceEvents",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_TimeStamp_DeviceId",
                table: "DeviceEvents",
                columns: new[] { "TimeStamp", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_TypeId",
                table: "DeviceEvents",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEventTypes_Name",
                table: "DeviceEventTypes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceEvents");

            migrationBuilder.DropTable(
                name: "DeviceEventTypes");
        }
    }
}
