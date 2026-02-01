using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIlmatieteenlaitosAsMeasurementSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MeasurementSources",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 2, "Ilmatieteenlaitos Open Data", "Ilmatieteenlaitos" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MeasurementSources",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
