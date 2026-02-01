using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMeasurementSourceUseCommunicationChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceMessages_MeasurementSources_SourceId",
                table: "DeviceMessages");

            migrationBuilder.DropTable(
                name: "MeasurementSources");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceMessages_CommunicationChannels_SourceId",
                table: "DeviceMessages",
                column: "SourceId",
                principalTable: "CommunicationChannels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceMessages_CommunicationChannels_SourceId",
                table: "DeviceMessages");

            migrationBuilder.CreateTable(
                name: "MeasurementSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
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
                    { 1, "Rest interface", "Rest" },
                    { 2, "Ilmatieteenlaitos Open Data", "Ilmatieteenlaitos" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceMessages_MeasurementSources_SourceId",
                table: "DeviceMessages",
                column: "SourceId",
                principalTable: "MeasurementSources",
                principalColumn: "Id");
        }
    }
}
