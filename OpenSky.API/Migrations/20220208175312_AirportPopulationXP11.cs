using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    public partial class AirportPopulationXP11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HasBeenPopulated",
                table: "Airports",
                newName: "HasBeenPopulatedMSFS");

            migrationBuilder.AddColumn<int>(
                name: "HasBeenPopulatedXP11",
                table: "Airports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Registry",
                table: "Aircraft",
                type: "varchar(12)",
                maxLength: 12,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasBeenPopulatedXP11",
                table: "Airports");

            migrationBuilder.RenameColumn(
                name: "HasBeenPopulatedMSFS",
                table: "Airports",
                newName: "HasBeenPopulated");

            migrationBuilder.AlterColumn<string>(
                name: "Registry",
                table: "Aircraft",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(12)",
                oldMaxLength: 12)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
