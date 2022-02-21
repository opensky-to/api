using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Aircraft manufacturers migration - step 1/2.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class AircraftManufacturers1 : Migration
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
        /// sushi.at, 20/02/2022.
        /// </remarks>
        /// <param name="migrationBuilder">
        /// The <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder" /> that will
        /// build the operations.
        /// </param>
        /// <seealso cref="Microsoft.EntityFrameworkCore.Migrations.Migration.Up(MigrationBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AircraftTypes_Airports_ManufacturerHomeAirportICAO",
                table: "AircraftTypes");

            migrationBuilder.DropIndex(
                name: "IX_AircraftTypes_ManufacturerHomeAirportICAO",
                table: "AircraftTypes");

            migrationBuilder.DropColumn(
                name: "Manufacturer",
                table: "AircraftTypes");

            migrationBuilder.DropColumn(
                name: "ManufacturerHomeAirportICAO",
                table: "AircraftTypes");

            migrationBuilder.AddColumn<string>(
                name: "AircraftManufacturerID",
                table: "AircraftTypes",
                type: "varchar(5)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AircraftManufacturers",
                columns: table => new
                {
                    ID = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftManufacturers", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AircraftManufacturerDeliveryLocations",
                columns: table => new
                {
                    AircraftTypeID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AirportICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ManufacturerID = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftManufacturerDeliveryLocations", x => new { x.ManufacturerID, x.AircraftTypeID, x.AirportICAO });
                    table.ForeignKey(
                        name: "FK_AircraftManufacturerDeliveryLocations_AircraftManufacturers_~",
                        column: x => x.ManufacturerID,
                        principalTable: "AircraftManufacturers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AircraftManufacturerDeliveryLocations_AircraftTypes_Aircraft~",
                        column: x => x.AircraftTypeID,
                        principalTable: "AircraftTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AircraftManufacturerDeliveryLocations_Airports_AirportICAO",
                        column: x => x.AirportICAO,
                        principalTable: "Airports",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftTypes_AircraftManufacturerID",
                table: "AircraftTypes",
                column: "AircraftManufacturerID");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftManufacturerDeliveryLocations_AircraftTypeID",
                table: "AircraftManufacturerDeliveryLocations",
                column: "AircraftTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftManufacturerDeliveryLocations_AirportICAO",
                table: "AircraftManufacturerDeliveryLocations",
                column: "AirportICAO");

            migrationBuilder.AddForeignKey(
                name: "FK_AircraftTypes_AircraftManufacturers_AircraftManufacturerID",
                table: "AircraftTypes",
                column: "AircraftManufacturerID",
                principalTable: "AircraftManufacturers",
                principalColumn: "ID",
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
        /// sushi.at, 20/02/2022.
        /// </remarks>
        /// <param name="migrationBuilder">
        /// The <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder" /> that will
        /// build the operations.
        /// </param>
        /// <seealso cref="Microsoft.EntityFrameworkCore.Migrations.Migration.Down(MigrationBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AircraftTypes_AircraftManufacturers_AircraftManufacturerID",
                table: "AircraftTypes");

            migrationBuilder.DropTable(
                name: "AircraftManufacturerDeliveryLocations");

            migrationBuilder.DropTable(
                name: "AircraftManufacturers");

            migrationBuilder.DropIndex(
                name: "IX_AircraftTypes_AircraftManufacturerID",
                table: "AircraftTypes");

            migrationBuilder.DropColumn(
                name: "AircraftManufacturerID",
                table: "AircraftTypes");

            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                table: "AircraftTypes",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerHomeAirportICAO",
                table: "AircraftTypes",
                type: "varchar(5)",
                maxLength: 5,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftTypes_ManufacturerHomeAirportICAO",
                table: "AircraftTypes",
                column: "ManufacturerHomeAirportICAO");

            migrationBuilder.AddForeignKey(
                name: "FK_AircraftTypes_Airports_ManufacturerHomeAirportICAO",
                table: "AircraftTypes",
                column: "ManufacturerHomeAirportICAO",
                principalTable: "Airports",
                principalColumn: "ICAO",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
