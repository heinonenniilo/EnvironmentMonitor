using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ScaleMinScaleMax : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ScaleMax",
                table: "Sensors",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ScaleMin",
                table: "Sensors",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScaleMax",
                table: "Sensors");

            migrationBuilder.DropColumn(
                name: "ScaleMin",
                table: "Sensors");
        }
    }
}
