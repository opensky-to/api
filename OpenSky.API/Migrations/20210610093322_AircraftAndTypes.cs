using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Aircraft type and basic aircraft migration.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class AircraftAndTypes : Migration
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
        /// sushi.at, 11/06/2021.
        /// </remarks>
        /// <param name="migrationBuilder">
        /// The <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder" /> that will
        /// build the operations.
        /// </param>
        /// <seealso cref="M:Microsoft.EntityFrameworkCore.Migrations.Migration.Up(MigrationBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AircraftTypes",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AtcModel = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AtcType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DetailedChecksDisabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EmptyWeight = table.Column<double>(type: "double", nullable: false),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EngineCount = table.Column<int>(type: "int", nullable: false),
                    EngineType = table.Column<int>(type: "int", nullable: false),
                    FlapsAvailable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FuelTotalCapacity = table.Column<double>(type: "double", nullable: false),
                    IsGearRetractable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsVanilla = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsVariantOf = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastEditedByID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxGrossWeight = table.Column<double>(type: "double", nullable: false),
                    MaxPrice = table.Column<int>(type: "int", nullable: false),
                    MinPrice = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NeedsCoPilot = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NextVersion = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Simulator = table.Column<int>(type: "int", nullable: false),
                    UploaderID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VersionNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftTypes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AircraftTypes_AircraftTypes_IsVariantOf",
                        column: x => x.IsVariantOf,
                        principalTable: "AircraftTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AircraftTypes_AircraftTypes_NextVersion",
                        column: x => x.NextVersion,
                        principalTable: "AircraftTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AircraftTypes_Users_LastEditedByID",
                        column: x => x.LastEditedByID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AircraftTypes_Users_UploaderID",
                        column: x => x.UploaderID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Aircraft",
                columns: table => new
                {
                    Registry = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AirportICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TypeID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aircraft", x => x.Registry);
                    table.ForeignKey(
                        name: "FK_Aircraft_AircraftTypes_TypeID",
                        column: x => x.TypeID,
                        principalTable: "AircraftTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Aircraft_Airports_AirportICAO",
                        column: x => x.AirportICAO,
                        principalTable: "Airports",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Aircraft_Users_OwnerID",
                        column: x => x.OwnerID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Aircraft_AirportICAO",
                table: "Aircraft",
                column: "AirportICAO");

            migrationBuilder.CreateIndex(
                name: "IX_Aircraft_OwnerID",
                table: "Aircraft",
                column: "OwnerID");

            migrationBuilder.CreateIndex(
                name: "IX_Aircraft_TypeID",
                table: "Aircraft",
                column: "TypeID");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftTypes_IsVariantOf",
                table: "AircraftTypes",
                column: "IsVariantOf");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftTypes_LastEditedByID",
                table: "AircraftTypes",
                column: "LastEditedByID");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftTypes_NextVersion",
                table: "AircraftTypes",
                column: "NextVersion");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftTypes_UploaderID",
                table: "AircraftTypes",
                column: "UploaderID");
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
        /// sushi.at, 11/06/2021.
        /// </remarks>
        /// <param name="migrationBuilder">
        /// The <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder" /> that will
        /// build the operations.
        /// </param>
        /// <seealso cref="M:Microsoft.EntityFrameworkCore.Migrations.Migration.Down(MigrationBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aircraft");

            migrationBuilder.DropTable(
                name: "AircraftTypes");
        }
    }
}
