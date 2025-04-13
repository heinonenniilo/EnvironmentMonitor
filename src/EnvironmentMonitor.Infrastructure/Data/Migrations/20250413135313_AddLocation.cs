using System;
using EnvironmentMonitor.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Devices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Sensors_Id_DeviceId",
                table: "Sensors",
                columns: new[] { "Id", "DeviceId" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Devices_Id_LocationId",
                table: "Devices",
                columns: new[] { "Id", "LocationId" });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Identifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.FillLocationDefaultRow();

            migrationBuilder.CreateTable(
                name: "LocationSensors",
                columns: table => new
                {
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    SensorId = table.Column<int>(type: "int", nullable: false),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationSensors", x => new { x.LocationId, x.SensorId, x.DeviceId });
                    table.ForeignKey(
                        name: "FK_LocationSensors_Devices_DeviceId_LocationId",
                        columns: x => new { x.DeviceId, x.LocationId },
                        principalTable: "Devices",
                        principalColumns: new[] { "Id", "LocationId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocationSensors_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationSensors_MeasurementTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "MeasurementTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationSensors_Sensors_SensorId_DeviceId",
                        columns: x => new { x.SensorId, x.DeviceId },
                        principalTable: "Sensors",
                        principalColumns: new[] { "Id", "DeviceId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_LocationId",
                table: "Devices",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Identifier",
                table: "Locations",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Name",
                table: "Locations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationSensors_DeviceId_LocationId",
                table: "LocationSensors",
                columns: new[] { "DeviceId", "LocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationSensors_SensorId_DeviceId",
                table: "LocationSensors",
                columns: new[] { "SensorId", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationSensors_TypeId",
                table: "LocationSensors",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Locations_LocationId",
                table: "Devices",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Locations_LocationId",
                table: "Devices");

            migrationBuilder.DropTable(
                name: "LocationSensors");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Sensors_Id_DeviceId",
                table: "Sensors");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Devices_Id_LocationId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_LocationId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Devices");
        }
    }
}
