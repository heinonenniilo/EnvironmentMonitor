using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentifierToSensorsAndPublicSensorsAddUniqueIndexToIdentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Identifier",
                table: "Sensors",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "newid()");

            migrationBuilder.AddColumn<Guid>(
                name: "Identifier",
                table: "PublicSensors",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "newid()");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_Identifier",
                table: "Sensors",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicSensors_Identifier",
                table: "PublicSensors",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_Identifier",
                table: "Devices",
                column: "Identifier",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sensors_Identifier",
                table: "Sensors");

            migrationBuilder.DropIndex(
                name: "IX_PublicSensors_Identifier",
                table: "PublicSensors");

            migrationBuilder.DropIndex(
                name: "IX_Devices_Identifier",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Identifier",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "Identifier",
                table: "PublicSensors");
        }
    }
}
