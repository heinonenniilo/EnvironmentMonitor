using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMqttCommunicationChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CommunicationChannels",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 4, "Mqtt", "Mqtt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CommunicationChannels",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
