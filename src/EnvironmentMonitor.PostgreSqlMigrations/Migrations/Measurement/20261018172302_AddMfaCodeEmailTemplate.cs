using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.PostgreSqlMigrations.Migrations.Measurement
{
    /// <inheritdoc />
    public partial class AddMfaCodeEmailTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "Id", "Created", "Message", "Name", "Title", "Updated" },
                values: new object[] { 4, null, null, "Multi factor authentication code", null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
