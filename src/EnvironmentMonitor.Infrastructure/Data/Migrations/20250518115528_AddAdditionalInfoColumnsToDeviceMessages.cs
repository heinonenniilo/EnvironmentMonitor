using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalInfoColumnsToDeviceMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Identifier",
                table: "DeviceMessages",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LoopCount",
                table: "DeviceMessages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "MessageCount",
                table: "DeviceMessages",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Uptime",
                table: "DeviceMessages",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Identifier",
                table: "DeviceMessages");

            migrationBuilder.DropColumn(
                name: "LoopCount",
                table: "DeviceMessages");

            migrationBuilder.DropColumn(
                name: "MessageCount",
                table: "DeviceMessages");

            migrationBuilder.DropColumn(
                name: "Uptime",
                table: "DeviceMessages");
        }
    }
}
