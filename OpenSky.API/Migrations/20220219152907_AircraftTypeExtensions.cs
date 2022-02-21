using Microsoft.EntityFrameworkCore.Migrations;
using OpenSky.API.DbModel.Enums;

namespace OpenSky.API.Migrations
{

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Aircraft type extensions migration.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class AircraftTypeExtensions : Migration
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
        /// sushi.at, 19/02/2022.
        /// </remarks>
        /// <param name="migrationBuilder">
        /// The <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder" /> that will
        /// build the operations.
        /// </param>
        /// <seealso cref="Microsoft.EntityFrameworkCore.Migrations.Migration.Up(MigrationBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "AircraftImage",
                table: "AircraftTypes",
                type: "longblob",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EngineModel",
                table: "AircraftTypes",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerHomeAirportICAO",
                table: "AircraftTypes",
                type: "varchar(5)",
                maxLength: 5,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "OverrideFuelType",
                table: "AircraftTypes",
                type: "int",
                nullable: false,
                defaultValue: (int)FuelType.NotUsed);

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
        /// sushi.at, 19/02/2022.
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
                name: "FK_AircraftTypes_Airports_ManufacturerHomeAirportICAO",
                table: "AircraftTypes");

            migrationBuilder.DropIndex(
                name: "IX_AircraftTypes_ManufacturerHomeAirportICAO",
                table: "AircraftTypes");

            migrationBuilder.DropColumn(
                name: "AircraftImage",
                table: "AircraftTypes");

            migrationBuilder.DropColumn(
                name: "EngineModel",
                table: "AircraftTypes");

            migrationBuilder.DropColumn(
                name: "ManufacturerHomeAirportICAO",
                table: "AircraftTypes");

            migrationBuilder.DropColumn(
                name: "OverrideFuelType",
                table: "AircraftTypes");
        }
    }
}
