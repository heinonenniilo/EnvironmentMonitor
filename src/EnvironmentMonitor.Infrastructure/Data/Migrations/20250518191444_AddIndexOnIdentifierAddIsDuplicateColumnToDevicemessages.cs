using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexOnIdentifierAddIsDuplicateColumnToDevicemessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDuplicate",
                table: "DeviceMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMessages_Identifier_DeviceId",
                table: "DeviceMessages",
                columns: new[] { "Identifier", "DeviceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeviceMessages_Identifier_DeviceId",
                table: "DeviceMessages");

            migrationBuilder.DropColumn(
                name: "IsDuplicate",
                table: "DeviceMessages");
        }
    }
}
