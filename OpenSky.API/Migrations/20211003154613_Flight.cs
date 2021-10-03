using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    public partial class Flight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AircraftRegistry = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AirspeedTrue = table.Column<double>(type: "double", nullable: true),
                    AlternateICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AutoSaveLog = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BankAngle = table.Column<double>(type: "double", nullable: false),
                    Completed = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Paused = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DestinationICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FlightLog = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FlightNumber = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FlightPhase = table.Column<int>(type: "int", nullable: false),
                    FuelTankCenter2Quantity = table.Column<double>(type: "double", nullable: true),
                    FuelTankCenter3Quantity = table.Column<double>(type: "double", nullable: true),
                    FuelTankCenterQuantity = table.Column<double>(type: "double", nullable: true),
                    FuelTankExternal1Quantity = table.Column<double>(type: "double", nullable: true),
                    FuelTankExternal2Quantity = table.Column<double>(type: "double", nullable: true),
                    FuelTankLeftAuxQuantity = table.Column<double>(type: "double", nullable: true),
                    FuelTankLeftMainQuantity = table.Column<double>(type: "double", nullable: true),
                    FuelTankLeftTipQuantity = table.Column<double>(type: "double", nullable: true),
                    FuelTankRightAuxQuantity = table.Column<double>(type: "double", nullable: true),
                    FuelTankRightMainQuantity = table.Column<double>(type: "double", nullable: true),
                    FuelTankRightTipQuantity = table.Column<double>(type: "double", nullable: true),
                    Heading = table.Column<double>(type: "double", nullable: true),
                    Latitude = table.Column<double>(type: "double", nullable: true),
                    Longitude = table.Column<double>(type: "double", nullable: true),
                    OnGround = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OperatorID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PitchAngle = table.Column<double>(type: "double", nullable: false),
                    RadioHeight = table.Column<double>(type: "double", nullable: true),
                    Started = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    VerticalSpeedSeconds = table.Column<double>(type: "double", nullable: false),
                    FuelGallons = table.Column<double>(type: "double", nullable: true),
                    FuelLoadingComplete = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PayloadLoadingComplete = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UtcOffset = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Flights_Aircraft_AircraftRegistry",
                        column: x => x.AircraftRegistry,
                        principalTable: "Aircraft",
                        principalColumn: "Registry",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flights_Airports_AlternateICAO",
                        column: x => x.AlternateICAO,
                        principalTable: "Airports",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flights_Airports_DestinationICAO",
                        column: x => x.DestinationICAO,
                        principalTable: "Airports",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flights_Airports_OriginICAO",
                        column: x => x.OriginICAO,
                        principalTable: "Airports",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flights_Users_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_AircraftRegistry",
                table: "Flights",
                column: "AircraftRegistry");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_AlternateICAO",
                table: "Flights",
                column: "AlternateICAO");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_DestinationICAO",
                table: "Flights",
                column: "DestinationICAO");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_OperatorID",
                table: "Flights",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_OriginICAO",
                table: "Flights",
                column: "OriginICAO");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Flights");
        }
    }
}
