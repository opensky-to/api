using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenSky.API.Migrations
{
    /// <inheritdoc />
    public partial class OnlineSeconds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OnlineNetworkConnectedSeconds",
                table: "Flights",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnlineNetworkConnectedSeconds",
                table: "Flights");
        }
    }
}
