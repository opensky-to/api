using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    public partial class JobsAndPayloads : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PersonalAccountBalance",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Job",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AssignedAirlineDispatcherID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OperatorAirlineID = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OperatorID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Job", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Job_Airlines_OperatorAirlineID",
                        column: x => x.OperatorAirlineID,
                        principalTable: "Airlines",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Job_Users_AssignedAirlineDispatcherID",
                        column: x => x.AssignedAirlineDispatcherID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Job_Users_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Payload",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AircraftRegistry = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AirportICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DestinationICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    JobID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Weight = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payload", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Payload_Aircraft_AircraftRegistry",
                        column: x => x.AircraftRegistry,
                        principalTable: "Aircraft",
                        principalColumn: "Registry",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payload_Airports_AirportICAO",
                        column: x => x.AirportICAO,
                        principalTable: "Airports",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payload_Airports_DestinationICAO",
                        column: x => x.DestinationICAO,
                        principalTable: "Airports",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payload_Job_JobID",
                        column: x => x.JobID,
                        principalTable: "Job",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_AssignedAirlinePilotID",
                table: "Flights",
                column: "AssignedAirlinePilotID");

            migrationBuilder.CreateIndex(
                name: "IX_Job_AssignedAirlineDispatcherID",
                table: "Job",
                column: "AssignedAirlineDispatcherID");

            migrationBuilder.CreateIndex(
                name: "IX_Job_OperatorAirlineID",
                table: "Job",
                column: "OperatorAirlineID");

            migrationBuilder.CreateIndex(
                name: "IX_Job_OperatorID",
                table: "Job",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_Payload_AircraftRegistry",
                table: "Payload",
                column: "AircraftRegistry");

            migrationBuilder.CreateIndex(
                name: "IX_Payload_AirportICAO",
                table: "Payload",
                column: "AirportICAO");

            migrationBuilder.CreateIndex(
                name: "IX_Payload_DestinationICAO",
                table: "Payload",
                column: "DestinationICAO");

            migrationBuilder.CreateIndex(
                name: "IX_Payload_JobID",
                table: "Payload",
                column: "JobID");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Users_AssignedAirlinePilotID",
                table: "Flights",
                column: "AssignedAirlinePilotID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Users_AssignedAirlinePilotID",
                table: "Flights");

            migrationBuilder.DropTable(
                name: "Payload");

            migrationBuilder.DropTable(
                name: "Job");

            migrationBuilder.DropIndex(
                name: "IX_Flights_AssignedAirlinePilotID",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "PersonalAccountBalance",
                table: "Users");
        }
    }
}
