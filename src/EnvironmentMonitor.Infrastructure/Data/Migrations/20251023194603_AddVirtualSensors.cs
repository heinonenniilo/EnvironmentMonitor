using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVirtualSensors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVirtual",
                table: "Sensors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVirtual",
                table: "Devices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "VirtualSensorRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VirtualSensorId = table.Column<int>(type: "int", nullable: false),
                    ValueSensorId = table.Column<int>(type: "int", nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualSensorRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VirtualSensorRows_MeasurementTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "MeasurementTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VirtualSensorRows_Sensors_ValueSensorId",
                        column: x => x.ValueSensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VirtualSensorRows_Sensors_VirtualSensorId",
                        column: x => x.VirtualSensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VirtualSensorRows_TypeId",
                table: "VirtualSensorRows",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VirtualSensorRows_ValueSensorId",
                table: "VirtualSensorRows",
                column: "ValueSensorId");

            migrationBuilder.CreateIndex(
                name: "IX_VirtualSensorRows_VirtualSensorId_ValueSensorId",
                table: "VirtualSensorRows",
                columns: new[] { "VirtualSensorId", "ValueSensorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VirtualSensorRows");

            migrationBuilder.DropColumn(
                name: "IsVirtual",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "IsVirtual",
                table: "Devices");
        }
    }
}
