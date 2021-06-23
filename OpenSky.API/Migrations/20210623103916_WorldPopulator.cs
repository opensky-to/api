using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    public partial class WorldPopulator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Country",
                table: "Airports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HasBeenPopulated",
                table: "Airports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PurchasePrice",
                table: "Aircraft",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RentPrice",
                table: "Aircraft",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "HasBeenPopulated",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "PurchasePrice",
                table: "Aircraft");

            migrationBuilder.DropColumn(
                name: "RentPrice",
                table: "Aircraft");
        }
    }
}
