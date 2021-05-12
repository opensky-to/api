using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Runway and approach with support for data imports migration.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class RunwayAndApproachWithDataImport : Migration
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
        /// sushi.at, 12/05/2021.
        /// </remarks>
        /// <param name="migrationBuilder">
        /// The <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder" /> that will
        /// build the operations.
        /// </param>
        /// <seealso cref="M:Microsoft.EntityFrameworkCore.Migrations.Migration.Up(MigrationBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Runways",
                table: "Airports",
                newName: "RunwayCount");

            migrationBuilder.AddColumn<string>(
                name: "HashCode",
                table: "Airports",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Approaches",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    AirportICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HashCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RunwayName = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Suffix = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Approaches", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Approaches_Airports_AirportICAO",
                        column: x => x.AirportICAO,
                        principalTable: "Airports",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DataImports",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Finished = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ImportDataSource = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LogText = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Started = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TotalRecordsProcessed = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataImports", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Runways",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    AirportICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Altitude = table.Column<int>(type: "int", nullable: false),
                    CenterLight = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EdgeLight = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HashCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Length = table.Column<int>(type: "int", nullable: false),
                    Surface = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Width = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Runways", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Runways_Airports_AirportICAO",
                        column: x => x.AirportICAO,
                        principalTable: "Airports",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RunwayEnds",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    ApproachLightSystem = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasClosedMarkings = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HashCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Heading = table.Column<double>(type: "double", nullable: false),
                    Latitude = table.Column<double>(type: "double", nullable: false),
                    LeftVasiPitch = table.Column<double>(type: "double", nullable: true),
                    LeftVasiType = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Longitude = table.Column<double>(type: "double", nullable: false),
                    Name = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OffsetThreshold = table.Column<int>(type: "int", nullable: false),
                    RightVasiPitch = table.Column<double>(type: "double", nullable: true),
                    RightVasiType = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RunwayID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunwayEnds", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RunwayEnds_Runways_RunwayID",
                        column: x => x.RunwayID,
                        principalTable: "Runways",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Approaches_AirportICAO",
                table: "Approaches",
                column: "AirportICAO");

            migrationBuilder.CreateIndex(
                name: "IX_RunwayEnds_RunwayID",
                table: "RunwayEnds",
                column: "RunwayID");

            migrationBuilder.CreateIndex(
                name: "IX_Runways_AirportICAO",
                table: "Runways",
                column: "AirportICAO");
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
        /// sushi.at, 12/05/2021.
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
                name: "Approaches");

            migrationBuilder.DropTable(
                name: "DataImports");

            migrationBuilder.DropTable(
                name: "RunwayEnds");

            migrationBuilder.DropTable(
                name: "Runways");

            migrationBuilder.DropColumn(
                name: "HashCode",
                table: "Airports");

            migrationBuilder.RenameColumn(
                name: "RunwayCount",
                table: "Airports",
                newName: "Runways");
        }
    }
}
