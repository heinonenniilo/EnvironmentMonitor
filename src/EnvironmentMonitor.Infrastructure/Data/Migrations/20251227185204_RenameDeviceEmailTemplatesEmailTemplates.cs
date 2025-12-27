using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameDeviceEmailTemplatesEmailTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Identifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            // Copy data from DeviceEmailTemplates to EmailTemplates
            migrationBuilder.Sql(@"
                INSERT INTO EmailTemplates (Id, Identifier, Title, Message, Name, CreatedUtc, Created, Updated)
                SELECT Id, Identifier, Title, Message, Name, CreatedUtc, Created, Updated
                FROM DeviceEmailTemplates
            ");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Identifier",
                table: "EmailTemplates",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Name",
                table: "EmailTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.DropTable(
                name: "DeviceEmailTemplates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceEmailTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Identifier = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Message = table.Column<string>(type: "nvarchar(max)", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceEmailTemplates", x => x.Id);
                });

            // Copy data back from EmailTemplates to DeviceEmailTemplates
            migrationBuilder.Sql(@"
                INSERT INTO DeviceEmailTemplates (Id, Identifier, Title, Message, Name, CreatedUtc, Created, Updated)
                SELECT Id, Identifier, Title, Message, Name, CreatedUtc, Created, Updated
                FROM EmailTemplates
            ");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEmailTemplates_Identifier",
                table: "DeviceEmailTemplates",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEmailTemplates_Name",
                table: "DeviceEmailTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.DropTable(
                name: "EmailTemplates");
        }
    }
}
