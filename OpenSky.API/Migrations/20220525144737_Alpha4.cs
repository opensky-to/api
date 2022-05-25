using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    using OpenSky.API.DbModel.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Merged migrations from Alpha1 through Alpha4.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Alpha4 : Migration
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
        ///                 This method must be overridden in each class the inherits from <see cref="T:Microsoft.EntityFrameworkCore.Migrations.Migration" />
        ///                 .
        ///             </para>
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/05/2022.
        /// </remarks>
        /// <param name="migrationBuilder">
        /// The <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder" /> that will
        /// build the operations.
        /// </param>
        /// <seealso cref="Microsoft.EntityFrameworkCore.Migrations.Migration.Up(MigrationBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
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
                name: "Airlines",
                columns: table => new
                {
                    ICAO = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountBalance = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AirportClientPackages",
                columns: table => new
                {
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Package = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PackageHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirportClientPackages", x => x.CreationTime);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    ICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Altitude = table.Column<int>(type: "int", nullable: false),
                    AtisFrequency = table.Column<int>(type: "int", nullable: true),
                    AvGasPrice = table.Column<float>(type: "float", nullable: false, defaultValue: 10f),
                    City = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GaRamps = table.Column<int>(type: "int", nullable: false),
                    Gates = table.Column<int>(type: "int", nullable: false),
                    HasAvGas = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HasBeenPopulatedMSFS = table.Column<int>(type: "int", nullable: false),
                    HasBeenPopulatedXP11 = table.Column<int>(type: "int", nullable: false),
                    HasJetFuel = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsClosed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsMilitary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    JetFuelPrice = table.Column<float>(type: "float", nullable: false, defaultValue: 5f),
                    Latitude = table.Column<double>(type: "double", nullable: false),
                    LongestRunwayLength = table.Column<int>(type: "int", nullable: false),
                    LongestRunwaySurface = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Longitude = table.Column<double>(type: "double", nullable: false),
                    MSFS = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PreviousSize = table.Column<int>(type: "int", nullable: true),
                    RunwayCount = table.Column<int>(type: "int", nullable: false),
                    S2Cell3 = table.Column<ulong>(type: "bigint unsigned", nullable: false, defaultValue: 0ul),
                    S2Cell4 = table.Column<ulong>(type: "bigint unsigned", nullable: false, defaultValue: 0ul),
                    S2Cell5 = table.Column<ulong>(type: "bigint unsigned", nullable: false, defaultValue: 0ul),
                    S2Cell6 = table.Column<ulong>(type: "bigint unsigned", nullable: false, defaultValue: 0ul),
                    S2Cell7 = table.Column<ulong>(type: "bigint unsigned", nullable: false, defaultValue: 0ul),
                    S2Cell8 = table.Column<ulong>(type: "bigint unsigned", nullable: false, defaultValue: 0ul),
                    S2Cell9 = table.Column<ulong>(type: "bigint unsigned", nullable: false, defaultValue: 0ul),
                    Size = table.Column<int>(type: "int", nullable: true),
                    SupportsSuper = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    TowerFrequency = table.Column<int>(type: "int", nullable: true),
                    UnicomFrequency = table.Column<int>(type: "int", nullable: true),
                    XP11 = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.ICAO);
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
                    ImportStatusJson = table.Column<string>(type: "longtext", nullable: true)
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
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Statistics",
                columns: table => new
                {
                    Key = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statistics", x => x.Key);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AirlineICAO = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AirlineIncomeShare = table.Column<int>(type: "int", nullable: true),
                    AirlineRank = table.Column<int>(type: "int", nullable: true),
                    AirlineSalary = table.Column<int>(type: "int", nullable: true),
                    BingMapsKey = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastLogin = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastLoginGeo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastLoginIP = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PersonalAccountBalance = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    ProfileImage = table.Column<byte[]>(type: "longblob", nullable: true),
                    RegisteredOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SimbriefUsername = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TokenRenewalCountryVerification = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecurityStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Airlines_AirlineICAO",
                        column: x => x.AirlineICAO,
                        principalTable: "Airlines",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Approaches",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    AirportICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false)
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
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AircraftTypes",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AircraftImage = table.Column<byte[]>(type: "longblob", nullable: true),
                    AtcModel = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AtcType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomAgentModule = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DetailedChecksDisabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EmptyWeight = table.Column<double>(type: "double", nullable: false),
                    Enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EngineCount = table.Column<int>(type: "int", nullable: false),
                    EngineModel = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EngineType = table.Column<int>(type: "int", nullable: false),
                    FlapsAvailable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FuelTotalCapacity = table.Column<double>(type: "double", nullable: false),
                    FuelWeightPerGallon = table.Column<double>(type: "double", nullable: false, defaultValue: -1),
                    IncludeInWorldPopulation = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IsGearRetractable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsHistoric = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IsVanilla = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsVariantOf = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastEditedByID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ManufacturerID = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxGrossWeight = table.Column<double>(type: "double", nullable: false),
                    MaxPayloadDeltaAllowed = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    MaxPrice = table.Column<int>(type: "int", nullable: false),
                    MinimumRunwayLength = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MinPrice = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NeedsCoPilot = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    NeedsFlightEngineer = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    NextVersion = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    OverrideFuelType = table.Column<int>(type: "int", nullable: false, defaultValue: (int)FuelType.NotUsed),
                    RequiresManualFuelling = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    RequiresManualLoading = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Simulator = table.Column<int>(type: "int", nullable: false),
                    UploaderID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VersionNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftTypes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AircraftTypes_AircraftManufacturers_ManufacturerID",
                        column: x => x.ManufacturerID,
                        principalTable: "AircraftManufacturers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
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
                name: "FinancialRecords",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AircraftRegistry = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AirlineID = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Expense = table.Column<long>(type: "bigint", nullable: false),
                    Income = table.Column<long>(type: "bigint", nullable: false),
                    ParentRecordID = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialRecords", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FinancialRecords_Airlines_AirlineID",
                        column: x => x.AirlineID,
                        principalTable: "Airlines",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialRecords_FinancialRecords_ParentRecordID",
                        column: x => x.ParentRecordID,
                        principalTable: "FinancialRecords",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialRecords_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Jobs",
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
                    OriginICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UserIdentifier = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Jobs_Airlines_OperatorAirlineID",
                        column: x => x.OperatorAirlineID,
                        principalTable: "Airlines",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jobs_Airports_OriginICAO",
                        column: x => x.OriginICAO,
                        principalTable: "Airports",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jobs_Users_AssignedAirlineDispatcherID",
                        column: x => x.AssignedAirlineDispatcherID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Jobs_Users_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OpenSkyTokens",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Expiry = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TokenGeo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TokenIP = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenSkyTokens", x => x.ID);
                    table.ForeignKey(
                        name: "FK_OpenSkyTokens_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderKey = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
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

            migrationBuilder.CreateTable(
                name: "Aircraft",
                columns: table => new
                {
                    Registry = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AirlineOwnerID = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AirportICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fuel = table.Column<double>(type: "double", nullable: false, defaultValue: 0),
                    FuellingUntil = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LifeTimeExpense = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    LifeTimeIncome = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    LoadingUntil = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PurchasePrice = table.Column<int>(type: "int", nullable: true),
                    RentPrice = table.Column<int>(type: "int", nullable: true),
                    TypeID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WarpingUntil = table.Column<DateTime>(type: "datetime(6)", nullable: true)
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
                        name: "FK_Aircraft_Airlines_AirlineOwnerID",
                        column: x => x.AirlineOwnerID,
                        principalTable: "Airlines",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Restrict);
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
                    AlternateRoute = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Altitude = table.Column<double>(type: "double", nullable: true),
                    AssignedAirlinePilotID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AutoSaveLog = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BankAngle = table.Column<double>(type: "double", nullable: false),
                    Completed = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DestinationICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispatcherID = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DispatcherRemarks = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FlightLog = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FlightNumber = table.Column<int>(type: "int", nullable: false),
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
                    GroundSpeed = table.Column<double>(type: "double", nullable: true),
                    Heading = table.Column<double>(type: "double", nullable: true),
                    LandedAtICAO = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastAutoSave = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastPositionReport = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Latitude = table.Column<double>(type: "double", nullable: true),
                    Longitude = table.Column<double>(type: "double", nullable: true),
                    OfpHtml = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    PlannedDepartureTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RadioHeight = table.Column<double>(type: "double", nullable: true),
                    Route = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Started = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TimeWarpTimeSavedSeconds = table.Column<int>(type: "int", nullable: false),
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
                        name: "FK_Flights_Airports_LandedAtICAO",
                        column: x => x.LandedAtICAO,
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
                        name: "FK_Flights_Users_AssignedAirlinePilotID",
                        column: x => x.AssignedAirlinePilotID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flights_Users_DispatcherID",
                        column: x => x.DispatcherID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flights_Users_OperatorID",
                        column: x => x.OperatorID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Payloads",
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
                    table.PrimaryKey("PK_Payloads", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Payloads_Aircraft_AircraftRegistry",
                        column: x => x.AircraftRegistry,
                        principalTable: "Aircraft",
                        principalColumn: "Registry",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payloads_Airports_AirportICAO",
                        column: x => x.AirportICAO,
                        principalTable: "Airports",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payloads_Airports_DestinationICAO",
                        column: x => x.DestinationICAO,
                        principalTable: "Airports",
                        principalColumn: "ICAO",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payloads_Jobs_JobID",
                        column: x => x.JobID,
                        principalTable: "Jobs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FlightNavlogFixes",
                columns: table => new
                {
                    FixNumber = table.Column<int>(type: "int", nullable: false),
                    FlightID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Ident = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<double>(type: "double", nullable: false),
                    Longitude = table.Column<double>(type: "double", nullable: false),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightNavlogFixes", x => new { x.FlightID, x.FixNumber });
                    table.ForeignKey(
                        name: "FK_FlightNavlogFixes_Flights_FlightID",
                        column: x => x.FlightID,
                        principalTable: "Flights",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                name: "IX_Aircraft_AirlineOwnerID",
                table: "Aircraft",
                column: "AirlineOwnerID");

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
                name: "IX_AircraftManufacturerDeliveryLocations_AircraftTypeID",
                table: "AircraftManufacturerDeliveryLocations",
                column: "AircraftTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftManufacturerDeliveryLocations_AirportICAO",
                table: "AircraftManufacturerDeliveryLocations",
                column: "AirportICAO");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftTypes_IsVariantOf",
                table: "AircraftTypes",
                column: "IsVariantOf");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftTypes_LastEditedByID",
                table: "AircraftTypes",
                column: "LastEditedByID");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftTypes_ManufacturerID",
                table: "AircraftTypes",
                column: "ManufacturerID");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftTypes_NextVersion",
                table: "AircraftTypes",
                column: "NextVersion");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftTypes_UploaderID",
                table: "AircraftTypes",
                column: "UploaderID");

            migrationBuilder.CreateIndex(
                name: "IX_AirlineShareHolders_UserID",
                table: "AirlineShareHolders",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_AirlineUserPermissions_UserID",
                table: "AirlineUserPermissions",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_S2Cell3",
                table: "Airports",
                column: "S2Cell3");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_S2Cell4",
                table: "Airports",
                column: "S2Cell4");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_S2Cell5",
                table: "Airports",
                column: "S2Cell5");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_S2Cell6",
                table: "Airports",
                column: "S2Cell6");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_S2Cell7",
                table: "Airports",
                column: "S2Cell7");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_S2Cell8",
                table: "Airports",
                column: "S2Cell8");

            migrationBuilder.CreateIndex(
                name: "IX_Airports_S2Cell9",
                table: "Airports",
                column: "S2Cell9");

            migrationBuilder.CreateIndex(
                name: "IX_Approaches_AirportICAO",
                table: "Approaches",
                column: "AirportICAO");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialRecords_AirlineID",
                table: "FinancialRecords",
                column: "AirlineID");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialRecords_ParentRecordID",
                table: "FinancialRecords",
                column: "ParentRecordID");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialRecords_UserID",
                table: "FinancialRecords",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_FlightPayloads_PayloadID",
                table: "FlightPayloads",
                column: "PayloadID");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_AircraftRegistry",
                table: "Flights",
                column: "AircraftRegistry");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_AlternateICAO",
                table: "Flights",
                column: "AlternateICAO");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_AssignedAirlinePilotID",
                table: "Flights",
                column: "AssignedAirlinePilotID");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_DestinationICAO",
                table: "Flights",
                column: "DestinationICAO");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_DispatcherID",
                table: "Flights",
                column: "DispatcherID");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_LandedAtICAO",
                table: "Flights",
                column: "LandedAtICAO");

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
                name: "IX_Jobs_AssignedAirlineDispatcherID",
                table: "Jobs",
                column: "AssignedAirlineDispatcherID");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OperatorAirlineID",
                table: "Jobs",
                column: "OperatorAirlineID");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OperatorID",
                table: "Jobs",
                column: "OperatorID");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OriginICAO",
                table: "Jobs",
                column: "OriginICAO");

            migrationBuilder.CreateIndex(
                name: "IX_OpenSkyTokens_UserID",
                table: "OpenSkyTokens",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Payloads_AircraftRegistry",
                table: "Payloads",
                column: "AircraftRegistry");

            migrationBuilder.CreateIndex(
                name: "IX_Payloads_AirportICAO",
                table: "Payloads",
                column: "AirportICAO");

            migrationBuilder.CreateIndex(
                name: "IX_Payloads_DestinationICAO",
                table: "Payloads",
                column: "DestinationICAO");

            migrationBuilder.CreateIndex(
                name: "IX_Payloads_JobID",
                table: "Payloads",
                column: "JobID");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RunwayEnds_RunwayID",
                table: "RunwayEnds",
                column: "RunwayID");

            migrationBuilder.CreateIndex(
                name: "IX_Runways_AirportICAO",
                table: "Runways",
                column: "AirportICAO");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AirlineICAO",
                table: "Users",
                column: "AirlineICAO");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);
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
        ///                 This method must be overridden in each class the inherits from <see cref="T:Microsoft.EntityFrameworkCore.Migrations.Migration" />
        ///                 if both 'up' and 'down' migrations are to be supported. If it is not
        ///                 overridden, then calling it will throw and it will not be possible to migrate
        ///                 in the 'down' direction.
        ///             </para>
        /// </summary>
        /// <remarks>
        /// sushi.at, 25/05/2022.
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
                name: "AircraftManufacturerDeliveryLocations");

            migrationBuilder.DropTable(
                name: "AirlineShareHolders");

            migrationBuilder.DropTable(
                name: "AirlineUserPermissions");

            migrationBuilder.DropTable(
                name: "AirportClientPackages");

            migrationBuilder.DropTable(
                name: "Approaches");

            migrationBuilder.DropTable(
                name: "DataImports");

            migrationBuilder.DropTable(
                name: "FinancialRecords");

            migrationBuilder.DropTable(
                name: "FlightNavlogFixes");

            migrationBuilder.DropTable(
                name: "FlightPayloads");

            migrationBuilder.DropTable(
                name: "OpenSkyTokens");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "RunwayEnds");

            migrationBuilder.DropTable(
                name: "Statistics");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "Flights");

            migrationBuilder.DropTable(
                name: "Payloads");

            migrationBuilder.DropTable(
                name: "Runways");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Aircraft");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "AircraftTypes");

            migrationBuilder.DropTable(
                name: "Airports");

            migrationBuilder.DropTable(
                name: "AircraftManufacturers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Airlines");
        }
    }
}
