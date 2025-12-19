using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackedEntityUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "DeviceQueuedCommands",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedUtc",
                table: "DeviceQueuedCommands",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "DeviceMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedUtc",
                table: "DeviceMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "DeviceContacts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedUtc",
                table: "DeviceContacts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "DeviceAttributes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedUtc",
                table: "DeviceAttributes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "DeviceAttachments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedUtc",
                table: "DeviceAttachments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "Attachments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedUtc",
                table: "Attachments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Updated",
                table: "DeviceQueuedCommands");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                table: "DeviceQueuedCommands");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "DeviceMessages");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                table: "DeviceMessages");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "DeviceContacts");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                table: "DeviceContacts");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "DeviceAttributes");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                table: "DeviceAttributes");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "DeviceAttachments");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                table: "DeviceAttachments");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "UpdatedUtc",
                table: "Attachments");
        }
    }
}
