using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Airlines and flights migration.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class AirlinesAndFlights : Migration
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
        /// sushi.at, 29/10/2021.
        /// </remarks>
        /// <param name="migrationBuilder">
        /// The <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder" /> that will
        /// build the operations.
        /// </param>
        /// <seealso cref="M:Microsoft.EntityFrameworkCore.Migrations.Migration.Up(MigrationBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AirlineICAO",
                table: "Users",
                type: "varchar(3)",
                maxLength: 3,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "AirlineIncomeShare",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AirlineRank",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AirlineSalary",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AirlineOwnerID",
                table: "Aircraft",
                type: "varchar(3)",
                maxLength: 3,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Airlines",
                columns: table => new
                {
                    ICAO = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Country = table.Column<int>(type: "int", nullable: false),
                    FounderID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FoundingDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IATA = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airlines", x => x.ICAO);
                    table.ForeignKey(
                        name: "FK_Airlines_Users_FounderID",
                        column: x => x.FounderID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AirlineShareHolders",
                columns: table => new
                {
                    AirlineICAO = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Shares = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirlineShareHolders", x => new { x.AirlineICAO, x.UserID });
                    table.ForeignKey(
                        name: "FK_AirlineShareHolders_Airlines_AirlineICAO",
                        column: x => x.AirlineICAO,
                        principalTable: "Airlines",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AirlineShareHolders_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AirlineUserPermissions",
                columns: table => new
                {
                    AirlineICAO = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Permission = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirlineUserPermissions", x => new { x.AirlineICAO, x.UserID });
                    table.ForeignKey(
                        name: "FK_AirlineUserPermissions_Airlines_AirlineICAO",
                        column: x => x.AirlineICAO,
                        principalTable: "Airlines",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AirlineUserPermissions_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                    AssignedAirlinePilotID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AutoSaveLog = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BankAngle = table.Column<double>(type: "double", nullable: false),
                    Completed = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DestinationICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FlightLog = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FlightNumber = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FlightPhase = table.Column<int>(type: "int", nullable: false),
                    FuelGallons = table.Column<double>(type: "double", nullable: true),
                    FuelLoadingComplete = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                    OperatorAirlineID = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OperatorID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Paused = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PayloadLoadingComplete = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PitchAngle = table.Column<double>(type: "double", nullable: false),
                    RadioHeight = table.Column<double>(type: "double", nullable: true),
                    Started = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UtcOffset = table.Column<double>(type: "double", nullable: false),
                    VerticalSpeedSeconds = table.Column<double>(type: "double", nullable: false)
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
                        name: "FK_Flights_Airlines_OperatorAirlineID",
                        column: x => x.OperatorAirlineID,
                        principalTable: "Airlines",
                        principalColumn: "ICAO",
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
                    table.ForeignKey(
                        name: "FK_Flights_Users_AssignedAirlinePilotID",
                        column: x => x.AssignedAirlinePilotID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AirlineICAO",
                table: "Users",
                column: "AirlineICAO");

            migrationBuilder.CreateIndex(
                name: "IX_Aircraft_AirlineOwnerID",
                table: "Aircraft",
                column: "AirlineOwnerID");

            migrationBuilder.CreateIndex(
                name: "IX_AirlineShareHolders_UserID",
                table: "AirlineShareHolders",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_AirlineUserPermissions_UserID",
                table: "AirlineUserPermissions",
                column: "UserID");

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
                name: "IX_Flights_OperatorAirlineID",
                table: "Flights",
                column: "OperatorAirlineID");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_OperatorID",
                table: "Flights",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_OriginICAO",
                table: "Flights",
                column: "OriginICAO");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_AssignedAirlinePilotID",
                table: "Flights",
                column: "AssignedAirlinePilotID");

            migrationBuilder.AddForeignKey(
                name: "FK_Aircraft_Airlines_AirlineOwnerID",
                table: "Aircraft",
                column: "AirlineOwnerID",
                principalTable: "Airlines",
                principalColumn: "ICAO",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Airlines_AirlineICAO",
                table: "Users",
                column: "AirlineICAO",
                principalTable: "Airlines",
                principalColumn: "ICAO",
                onDelete: ReferentialAction.Restrict);
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
        /// sushi.at, 29/10/2021.
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
                name: "FK_Aircraft_Airlines_AirlineOwnerID",
                table: "Aircraft");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Airlines_AirlineICAO",
                table: "Users");

            migrationBuilder.DropTable(
                name: "AirlineShareHolders");

            migrationBuilder.DropTable(
                name: "AirlineUserPermissions");

            migrationBuilder.DropTable(
                name: "Flights");

            migrationBuilder.DropTable(
                name: "Airlines");

            migrationBuilder.DropIndex(
                name: "IX_Users_AirlineICAO",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Aircraft_AirlineOwnerID",
                table: "Aircraft");

            migrationBuilder.DropColumn(
                name: "AirlineICAO",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AirlineIncomeShare",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AirlineRank",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AirlineSalary",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AirlineOwnerID",
                table: "Aircraft");
        }
    }
}
