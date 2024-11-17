using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasurementType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_SensorTypes_TypeId",
                table: "Sensors");

            migrationBuilder.DeleteData(
                table: "SensorTypes",
                keyColumn: "Id",
                keyValue: 0);

            migrationBuilder.DeleteData(
                table: "SensorTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SensorTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AlterColumn<int>(
                name: "TypeId",
                table: "Sensors",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql(@"
                Update Sensors set TypeId = null;
            ");

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Measurements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MeasurementTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "MeasurementTypes",
                columns: new[] { "Id", "Name", "Unit" },
                values: new object[,]
                {
                    { 0, "Undefined", "-" },
                    { 1, "Temperature", "C" },
                    { 2, "Humidity", "%" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_TypeId",
                table: "Measurements",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementTypes_Name",
                table: "MeasurementTypes",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Measurements_MeasurementTypes_TypeId",
                table: "Measurements",
                column: "TypeId",
                principalTable: "MeasurementTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_SensorTypes_TypeId",
                table: "Sensors",
                column: "TypeId",
                principalTable: "SensorTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Measurements_MeasurementTypes_TypeId",
                table: "Measurements");

            migrationBuilder.DropForeignKey(
                name: "FK_Sensors_SensorTypes_TypeId",
                table: "Sensors");

            migrationBuilder.DropTable(
                name: "MeasurementTypes");

            migrationBuilder.DropIndex(
                name: "IX_Measurements_TypeId",
                table: "Measurements");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Measurements");

            migrationBuilder.AlterColumn<int>(
                name: "TypeId",
                table: "Sensors",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "SensorTypes",
                columns: new[] { "Id", "Name", "Unit" },
                values: new object[,]
                {
                    { 0, "Undefined", "-" },
                    { 1, "Temperature", "C" },
                    { 2, "Humidity", "%" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Sensors_SensorTypes_TypeId",
                table: "Sensors",
                column: "TypeId",
                principalTable: "SensorTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
