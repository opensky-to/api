using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Flight payloads migration.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class FlightPayloads : Migration
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// <para>
        ///                 Builds the operations that will migrate the database 'up'.
        ///             </para>
        /// <para>
        ///                 That is, builds the operations that will take the database from the state
        ///                 left in by the previous migration so that it is up-to-date with regard to
        ///                 this migration.
        ///             </para>
        /// <para>
        ///                 This method must be overridden in each class the inherits from
        ///                 <see cref="T:Microsoft.EntityFrameworkCore.Migrations.Migration" />.
        ///             </para>
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/12/2021.
        /// </remarks>
        /// <param name="migrationBuilder">
        /// The <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder" /> that will
        /// build the operations.
        /// </param>
        /// <seealso cref="M:Microsoft.EntityFrameworkCore.Migrations.Migration.Up(MigrationBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Job_Airlines_OperatorAirlineID",
                table: "Job");

            migrationBuilder.DropForeignKey(
                name: "FK_Job_Airports_OriginICAO",
                table: "Job");

            migrationBuilder.DropForeignKey(
                name: "FK_Job_Users_AssignedAirlineDispatcherID",
                table: "Job");

            migrationBuilder.DropForeignKey(
                name: "FK_Job_Users_OperatorID",
                table: "Job");

            migrationBuilder.DropForeignKey(
                name: "FK_Payload_Aircraft_AircraftRegistry",
                table: "Payload");

            migrationBuilder.DropForeignKey(
                name: "FK_Payload_Airports_AirportICAO",
                table: "Payload");

            migrationBuilder.DropForeignKey(
                name: "FK_Payload_Airports_DestinationICAO",
                table: "Payload");

            migrationBuilder.DropForeignKey(
                name: "FK_Payload_Job_JobID",
                table: "Payload");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payload",
                table: "Payload");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Job",
                table: "Job");

            migrationBuilder.RenameTable(
                name: "Payload",
                newName: "Payloads");

            migrationBuilder.RenameTable(
                name: "Job",
                newName: "Jobs");

            migrationBuilder.RenameIndex(
                name: "IX_Payload_JobID",
                table: "Payloads",
                newName: "IX_Payloads_JobID");

            migrationBuilder.RenameIndex(
                name: "IX_Payload_DestinationICAO",
                table: "Payloads",
                newName: "IX_Payloads_DestinationICAO");

            migrationBuilder.RenameIndex(
                name: "IX_Payload_AirportICAO",
                table: "Payloads",
                newName: "IX_Payloads_AirportICAO");

            migrationBuilder.RenameIndex(
                name: "IX_Payload_AircraftRegistry",
                table: "Payloads",
                newName: "IX_Payloads_AircraftRegistry");

            migrationBuilder.RenameIndex(
                name: "IX_Job_OriginICAO",
                table: "Jobs",
                newName: "IX_Jobs_OriginICAO");

            migrationBuilder.RenameIndex(
                name: "IX_Job_OperatorID",
                table: "Jobs",
                newName: "IX_Jobs_OperatorID");

            migrationBuilder.RenameIndex(
                name: "IX_Job_OperatorAirlineID",
                table: "Jobs",
                newName: "IX_Jobs_OperatorAirlineID");

            migrationBuilder.RenameIndex(
                name: "IX_Job_AssignedAirlineDispatcherID",
                table: "Jobs",
                newName: "IX_Jobs_AssignedAirlineDispatcherID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payloads",
                table: "Payloads",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Jobs",
                table: "Jobs",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "FlightPayloads",
                columns: table => new
                {
                    FlightID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PayloadID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightPayloads", x => new { x.FlightID, x.PayloadID });
                    table.ForeignKey(
                        name: "FK_FlightPayloads_Flights_FlightID",
                        column: x => x.FlightID,
                        principalTable: "Flights",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlightPayloads_Payloads_PayloadID",
                        column: x => x.PayloadID,
                        principalTable: "Payloads",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_FlightPayloads_PayloadID",
                table: "FlightPayloads",
                column: "PayloadID");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Airlines_OperatorAirlineID",
                table: "Jobs",
                column: "OperatorAirlineID",
                principalTable: "Airlines",
                principalColumn: "ICAO",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Airports_OriginICAO",
                table: "Jobs",
                column: "OriginICAO",
                principalTable: "Airports",
                principalColumn: "ICAO",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Users_AssignedAirlineDispatcherID",
                table: "Jobs",
                column: "AssignedAirlineDispatcherID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Users_OperatorID",
                table: "Jobs",
                column: "OperatorID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payloads_Aircraft_AircraftRegistry",
                table: "Payloads",
                column: "AircraftRegistry",
                principalTable: "Aircraft",
                principalColumn: "Registry",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payloads_Airports_AirportICAO",
                table: "Payloads",
                column: "AirportICAO",
                principalTable: "Airports",
                principalColumn: "ICAO",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payloads_Airports_DestinationICAO",
                table: "Payloads",
                column: "DestinationICAO",
                principalTable: "Airports",
                principalColumn: "ICAO",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payloads_Jobs_JobID",
                table: "Payloads",
                column: "JobID",
                principalTable: "Jobs",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// <para>
        ///                 Builds the operations that will migrate the database 'down'.
        ///             </para>
        /// <para>
        ///                 That is, builds the operations that will take the database from the state
        ///                 left in by this migration so that it returns to the state that it was in
        ///                 before this migration was applied.
        ///             </para>
        /// <para>
        ///                 This method must be overridden in each class the inherits from
        ///                 <see cref="T:Microsoft.EntityFrameworkCore.Migrations.Migration" /> if both
        ///                 'up' and 'down' migrations are to be supported. If it is not overridden, then
        ///                 calling it will throw and it will not be possible to migrate in the 'down'
        ///                 direction.
        ///             </para>
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/12/2021.
        /// </remarks>
        /// <param name="migrationBuilder">
        /// The <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder" /> that will
        /// build the operations.
        /// </param>
        /// <seealso cref="M:Microsoft.EntityFrameworkCore.Migrations.Migration.Down(MigrationBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Airlines_OperatorAirlineID",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Airports_OriginICAO",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Users_AssignedAirlineDispatcherID",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Users_OperatorID",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Payloads_Aircraft_AircraftRegistry",
                table: "Payloads");

            migrationBuilder.DropForeignKey(
                name: "FK_Payloads_Airports_AirportICAO",
                table: "Payloads");

            migrationBuilder.DropForeignKey(
                name: "FK_Payloads_Airports_DestinationICAO",
                table: "Payloads");

            migrationBuilder.DropForeignKey(
                name: "FK_Payloads_Jobs_JobID",
                table: "Payloads");

            migrationBuilder.DropTable(
                name: "FlightPayloads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payloads",
                table: "Payloads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Jobs",
                table: "Jobs");

            migrationBuilder.RenameTable(
                name: "Payloads",
                newName: "Payload");

            migrationBuilder.RenameTable(
                name: "Jobs",
                newName: "Job");

            migrationBuilder.RenameIndex(
                name: "IX_Payloads_JobID",
                table: "Payload",
                newName: "IX_Payload_JobID");

            migrationBuilder.RenameIndex(
                name: "IX_Payloads_DestinationICAO",
                table: "Payload",
                newName: "IX_Payload_DestinationICAO");

            migrationBuilder.RenameIndex(
                name: "IX_Payloads_AirportICAO",
                table: "Payload",
                newName: "IX_Payload_AirportICAO");

            migrationBuilder.RenameIndex(
                name: "IX_Payloads_AircraftRegistry",
                table: "Payload",
                newName: "IX_Payload_AircraftRegistry");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_OriginICAO",
                table: "Job",
                newName: "IX_Job_OriginICAO");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_OperatorID",
                table: "Job",
                newName: "IX_Job_OperatorID");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_OperatorAirlineID",
                table: "Job",
                newName: "IX_Job_OperatorAirlineID");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_AssignedAirlineDispatcherID",
                table: "Job",
                newName: "IX_Job_AssignedAirlineDispatcherID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payload",
                table: "Payload",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Job",
                table: "Job",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Job_Airlines_OperatorAirlineID",
                table: "Job",
                column: "OperatorAirlineID",
                principalTable: "Airlines",
                principalColumn: "ICAO",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Job_Airports_OriginICAO",
                table: "Job",
                column: "OriginICAO",
                principalTable: "Airports",
                principalColumn: "ICAO",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Job_Users_AssignedAirlineDispatcherID",
                table: "Job",
                column: "AssignedAirlineDispatcherID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Job_Users_OperatorID",
                table: "Job",
                column: "OperatorID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payload_Aircraft_AircraftRegistry",
                table: "Payload",
                column: "AircraftRegistry",
                principalTable: "Aircraft",
                principalColumn: "Registry",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payload_Airports_AirportICAO",
                table: "Payload",
                column: "AirportICAO",
                principalTable: "Airports",
                principalColumn: "ICAO",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payload_Airports_DestinationICAO",
                table: "Payload",
                column: "DestinationICAO",
                principalTable: "Airports",
                principalColumn: "ICAO",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payload_Job_JobID",
                table: "Payload",
                column: "JobID",
                principalTable: "Job",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
