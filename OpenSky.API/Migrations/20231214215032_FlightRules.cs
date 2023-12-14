using Microsoft.EntityFrameworkCore.Migrations;
using OpenSky.API.DbModel.Enums;

#nullable disable

namespace OpenSky.API.Migrations
{
    /// <inheritdoc />
    public partial class FlightRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FlightRule",
                table: "Flights",
                type: "int",
                nullable: false,
                defaultValue: FlightRule.IFR);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlightRule",
                table: "Flights");
        }
    }
}
