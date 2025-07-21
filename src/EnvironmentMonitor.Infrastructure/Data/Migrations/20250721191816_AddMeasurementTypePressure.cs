using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasurementTypePressure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MeasurementTypes",
                columns: new[] { "Id", "Name", "Unit" },
                values: new object[] { 5, "Pressure", "-" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MeasurementTypes",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
