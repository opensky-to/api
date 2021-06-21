using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    public partial class AicraftPurchaseAndRentPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "PurchasePrice",
                table: "Aircraft");

            migrationBuilder.DropColumn(
                name: "RentPrice",
                table: "Aircraft");
        }
    }
}
