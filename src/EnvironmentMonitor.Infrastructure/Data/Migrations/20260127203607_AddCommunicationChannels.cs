using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunicationChannels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommunicationChannelId",
                table: "Devices",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CommunicationChannels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationChannels", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CommunicationChannels",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 0, "IoT Hub", "IotHub" },
                    { 1, "Rest API", "RestApi" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_CommunicationChannelId",
                table: "Devices",
                column: "CommunicationChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_CommunicationChannels_CommunicationChannelId",
                table: "Devices",
                column: "CommunicationChannelId",
                principalTable: "CommunicationChannels",
                principalColumn: "Id");

            migrationBuilder.Sql("UPDATE Devices SET CommunicationChannelId = 0 WHERE CommunicationChannelId IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_CommunicationChannels_CommunicationChannelId",
                table: "Devices");

            migrationBuilder.DropTable(
                name: "CommunicationChannels");

            migrationBuilder.DropIndex(
                name: "IX_Devices_CommunicationChannelId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "CommunicationChannelId",
                table: "Devices");
        }
    }
}
