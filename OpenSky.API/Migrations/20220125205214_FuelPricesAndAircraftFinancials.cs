﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Fuel prices for airports and financial records aircraft migration.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class FuelPricesAndAircraftFinancials : Migration
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
        /// sushi.at, 25/01/2022.
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
                name: "AircraftRegistry",
                table: "FinancialRecords",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<float>(
                name: "AvGasPrice",
                table: "Airports",
                type: "float",
                nullable: false,
                defaultValue: 10f);

            migrationBuilder.AddColumn<float>(
                name: "JetFuelPrice",
                table: "Airports",
                type: "float",
                nullable: false,
                defaultValue: 5f);
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
        /// sushi.at, 25/01/2022.
        /// </remarks>
        /// <param name="migrationBuilder">
        /// The <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder" /> that will
        /// build the operations.
        /// </param>
        /// <seealso cref="M:Microsoft.EntityFrameworkCore.Migrations.Migration.Down(MigrationBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AircraftRegistry",
                table: "FinancialRecords");

            migrationBuilder.DropColumn(
                name: "AvGasPrice",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "JetFuelPrice",
                table: "Airports");
        }
    }
}
