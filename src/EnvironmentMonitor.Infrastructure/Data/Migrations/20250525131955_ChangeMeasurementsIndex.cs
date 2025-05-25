using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMeasurementsIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Measurements_SensorId_Timestamp",
                table: "Measurements");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_SensorId_Timestamp",
                table: "Measurements",
                columns: new[] { "SensorId", "Timestamp" })
                .Annotation("SqlServer:Include", new[] { "Value", "TypeId", "TimestampUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Measurements_SensorId_Timestamp",
                table: "Measurements");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_SensorId_Timestamp",
                table: "Measurements",
                columns: new[] { "SensorId", "Timestamp" });
        }
    }
}
