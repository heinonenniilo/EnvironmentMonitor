using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationVisible : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Visible",
                table: "Locations",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Visible",
                table: "Locations");
        }
    }
}
