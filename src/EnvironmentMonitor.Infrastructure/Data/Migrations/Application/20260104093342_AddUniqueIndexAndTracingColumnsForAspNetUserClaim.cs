using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations.Application
{
    /// <inheritdoc />
    public partial class AddUniqueIndexAndTracingColumnsForAspNetUserClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ClaimValue",
                schema: "application",
                table: "AspNetUserClaims",
                type: "nvarchar(125)",
                maxLength: 125,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClaimType",
                schema: "application",
                table: "AspNetUserClaims",
                type: "nvarchar(125)",
                maxLength: 125,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                schema: "application",
                table: "AspNetUserClaims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                schema: "application",
                table: "AspNetUserClaims",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                schema: "application",
                table: "AspNetUserClaims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedById",
                schema: "application",
                table: "AspNetUserClaims",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedUtc",
                schema: "application",
                table: "AspNetUserClaims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_ClaimType_ClaimValue_UserId",
                schema: "application",
                table: "AspNetUserClaims",
                columns: new[] { "ClaimType", "ClaimValue", "UserId" },
                unique: true,
                filter: "[ClaimType] IS NOT NULL AND [ClaimValue] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UpdatedById",
                schema: "application",
                table: "AspNetUserClaims",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UpdatedById",
                schema: "application",
                table: "AspNetUserClaims",
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
                name: "FK_AspNetUserClaims_AspNetUsers_UpdatedById",
                schema: "application",
                table: "AspNetUserClaims");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserClaims_ClaimType_ClaimValue_UserId",
                schema: "application",
                table: "AspNetUserClaims");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserClaims_UpdatedById",
                schema: "application",
                table: "AspNetUserClaims");

            migrationBuilder.DropColumn(
                name: "Created",
                schema: "application",
                table: "AspNetUserClaims");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                schema: "application",
                table: "AspNetUserClaims");

            migrationBuilder.DropColumn(
                name: "Updated",
                schema: "application",
                table: "AspNetUserClaims");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                schema: "application",
                table: "AspNetUserClaims");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                schema: "application",
                table: "AspNetUserClaims");

            migrationBuilder.AlterColumn<string>(
                name: "ClaimValue",
                schema: "application",
                table: "AspNetUserClaims",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(125)",
                oldMaxLength: 125,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClaimType",
                schema: "application",
                table: "AspNetUserClaims",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(125)",
                oldMaxLength: 125,
                oldNullable: true);
        }
    }
}
