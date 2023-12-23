using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenSky.API.Migrations
{
    /// <inheritdoc />
    public partial class DisableAutoRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DisableAutoRegistry",
                table: "AircraftTypes",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisableAutoRegistry",
                table: "AircraftTypes");
        }
    }
}
