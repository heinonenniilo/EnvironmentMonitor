using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsImageFromAttachmentAddIsDeviceImageToDeviceAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsImage",
                table: "Attachments");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeviceImage",
                table: "DeviceAttachments",
                type: "bit",
                nullable: false,
                defaultValue: false);


            migrationBuilder.Sql(@"
update da
set 
    IsDeviceImage = 1
from 
    DeviceAttachments da
join 
    Attachments a on a.Id = da.AttachmentId
where 
    a.ContentType like '%image%'
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeviceImage",
                table: "DeviceAttachments");

            migrationBuilder.AddColumn<bool>(
                name: "IsImage",
                table: "Attachments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
