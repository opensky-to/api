using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    public partial class Airport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    ICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Altitude = table.Column<int>(type: "int", nullable: false),
                    AtisFrequency = table.Column<int>(type: "int", nullable: true),
                    City = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GaRamps = table.Column<int>(type: "int", nullable: false),
                    Gates = table.Column<int>(type: "int", nullable: false),
                    HasAvGas = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HasJetFuel = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsClosed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsMilitary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Latitude = table.Column<double>(type: "double", nullable: false),
                    LongestRunwayLength = table.Column<int>(type: "int", nullable: false),
                    LongestRunwaySurface = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Longitude = table.Column<double>(type: "double", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Runways = table.Column<int>(type: "int", nullable: false),
                    TowerFrequency = table.Column<int>(type: "int", nullable: true),
                    UnicomFrequency = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.ICAO);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Airports");
        }
    }
}
