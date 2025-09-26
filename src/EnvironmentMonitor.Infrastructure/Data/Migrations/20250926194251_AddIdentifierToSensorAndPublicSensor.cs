using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentifierToSensorAndPublicSensor : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Identifier",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "Identifier",
                table: "PublicSensors");
        }
    }
}
