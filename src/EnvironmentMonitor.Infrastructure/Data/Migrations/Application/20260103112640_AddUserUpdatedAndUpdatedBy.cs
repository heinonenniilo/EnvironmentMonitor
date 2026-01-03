using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations.Application
{
    /// <inheritdoc />
    public partial class AddUserUpdatedAndUpdatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                schema: "application",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                schema: "application",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                schema: "application",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                schema: "application",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedUtc",
                schema: "application",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UpdatedById",
                schema: "application",
                table: "AspNetUsers",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_UpdatedById",
                schema: "application",
                table: "AspNetUsers",
                column: "UpdatedById",
                principalSchema: "application",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_UpdatedById",
                schema: "application",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_UpdatedById",
                schema: "application",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Created",
                schema: "application",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                schema: "application",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Updated",
                schema: "application",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                schema: "application",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                schema: "application",
                table: "AspNetUsers");
        }
    }
}
