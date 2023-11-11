using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Aircraft maintenance migration.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class AircraftMaintenance : Migration
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Builds the operations that will migrate the database 'up'.
        /// </summary>
        /// <remarks>
        /// <para>
        ///                 That is, builds the operations that will take the database from the state
        ///                 left in by the previous migration so that it is up-to-date with regard to
        ///                 this migration.
        ///             </para>
        /// <para>
        ///                 This method must be overridden in each class that inherits from <see cref="T:Microsoft.EntityFrameworkCore.Migrations.Migration" />.
        ///                 
        ///             </para>
        /// <para>
        ///                 See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see>
        ///                 for more information and examples.
        ///             </para>
        /// </remarks>
        /// <param name="migrationBuilder">
        /// The <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder" /> that will
        /// build the operations.
        /// </param>
        /// <seealso cref="Microsoft.EntityFrameworkCore.Migrations.Migration.Up(MigrationBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AirframeHours",
                table: "Aircraft",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Engine1Hours",
                table: "Aircraft",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Engine2Hours",
                table: "Aircraft",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Engine3Hours",
                table: "Aircraft",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Engine4Hours",
                table: "Aircraft",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "AircraftMaintenances",
                columns: table => new
                {
                    AircraftRegistry = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecordNumber = table.Column<int>(type: "int", nullable: false),
                    PlannedAtAirportICAO = table.Column<string>(type: "varchar(5)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlannedAtICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlannedFor = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RequiredManHours = table.Column<int>(type: "int", nullable: false),
                    Started = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Technicians = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftMaintenances", x => new { x.AircraftRegistry, x.RecordNumber });
                    table.ForeignKey(
                        name: "FK_AircraftMaintenances_Aircraft_AircraftRegistry",
                        column: x => x.AircraftRegistry,
                        principalTable: "Aircraft",
                        principalColumn: "Registry",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AircraftMaintenances_Airports_PlannedAtAirportICAO",
                        column: x => x.PlannedAtAirportICAO,
                        principalTable: "Airports",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftMaintenances_PlannedAtAirportICAO",
                table: "AircraftMaintenances",
                column: "PlannedAtAirportICAO");
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Builds the operations that will migrate the database 'down'.
        /// </summary>
        /// <remarks>
        /// <para>
        ///                 That is, builds the operations that will take the database from the state
        ///                 left in by this migration so that it returns to the state that it was in
        ///                 before this migration was applied.
        ///             </para>
        /// <para>
        ///                 This method must be overridden in each class that inherits from <see cref="T:Microsoft.EntityFrameworkCore.Migrations.Migration" />
        ///                 if both 'up' and 'down' migrations are to be supported. If it is not
        ///                 overridden, then calling it will throw and it will not be possible to migrate
        ///                 in the 'down' direction.
        ///             </para>
        /// <para>
        ///                 See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see>
        ///                 for more information and examples.
        ///             </para>
        /// </remarks>
        /// <param name="migrationBuilder">
        /// The <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder" /> that will
        /// build the operations.
        /// </param>
        /// <seealso cref="Microsoft.EntityFrameworkCore.Migrations.Migration.Down(MigrationBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AircraftMaintenances");

            migrationBuilder.DropColumn(
                name: "AirframeHours",
                table: "Aircraft");

            migrationBuilder.DropColumn(
                name: "Engine1Hours",
                table: "Aircraft");

            migrationBuilder.DropColumn(
                name: "Engine2Hours",
                table: "Aircraft");

            migrationBuilder.DropColumn(
                name: "Engine3Hours",
                table: "Aircraft");

            migrationBuilder.DropColumn(
                name: "Engine4Hours",
                table: "Aircraft");
        }
    }
}
