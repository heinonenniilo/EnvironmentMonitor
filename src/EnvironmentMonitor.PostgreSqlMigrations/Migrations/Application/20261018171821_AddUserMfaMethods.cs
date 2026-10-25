using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EnvironmentMonitor.PostgreSqlMigrations.Migrations.Application
{
    /// <inheritdoc />
    public partial class AddUserMfaMethods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserMfaMethods",
                schema: "application",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    ConfirmedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisabledUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "LOCALTIMESTAMP"),
                    UpdatedById = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMfaMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMfaMethods_AspNetUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalSchema: "application",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserMfaMethods_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "application",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMfaMethods_UpdatedById",
                schema: "application",
                table: "UserMfaMethods",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserMfaMethods_UserId_Method",
                schema: "application",
                table: "UserMfaMethods",
                columns: new[] { "UserId", "Method" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMfaMethods",
                schema: "application");
        }
    }
}
