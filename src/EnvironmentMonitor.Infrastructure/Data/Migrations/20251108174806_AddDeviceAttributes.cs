using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceAttributeTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAttributeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceAttributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeStampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceAttributes_DeviceAttributeTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DeviceAttributeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceAttributes_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DeviceAttributeTypes",
                columns: new[] { "Id", "Description", "Name", "Type" },
                values: new object[,]
                {
                    { 0, "Motion control status. 0=AlwaysOff,1=AlwaysOn,2=MotionControl", "MotionControlStatus", "int" },
                    { 1, "Output delay in ms when motion control is on.", "OnDelay", "int" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAttributes_DeviceId_TypeId",
                table: "DeviceAttributes",
                columns: new[] { "DeviceId", "TypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAttributes_TimeStamp_DeviceId",
                table: "DeviceAttributes",
                columns: new[] { "TimeStamp", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAttributes_TypeId",
                table: "DeviceAttributes",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAttributeTypes_Name",
                table: "DeviceAttributeTypes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceAttributes");

            migrationBuilder.DropTable(
                name: "DeviceAttributeTypes");
        }
    }
}
