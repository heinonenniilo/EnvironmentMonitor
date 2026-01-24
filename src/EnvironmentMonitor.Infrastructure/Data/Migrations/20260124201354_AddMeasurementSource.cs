using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasurementSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceId",
                table: "DeviceMessages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MeasurementSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementSources", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "MeasurementSources",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 0, "IoT Hub", "IotHub" },
                    { 1, "Rest interface", "Rest" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMessages_SourceId",
                table: "DeviceMessages",
                column: "SourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceMessages_MeasurementSources_SourceId",
                table: "DeviceMessages",
                column: "SourceId",
                principalTable: "MeasurementSources",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceMessages_MeasurementSources_SourceId",
                table: "DeviceMessages");

            migrationBuilder.DropTable(
                name: "MeasurementSources");

            migrationBuilder.DropIndex(
                name: "IX_DeviceMessages_SourceId",
                table: "DeviceMessages");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "DeviceMessages");
        }
    }
}
