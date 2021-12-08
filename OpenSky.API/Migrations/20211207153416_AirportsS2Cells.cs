using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Airports S2 Cell IDs migration.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class AirportsS2Cells : Migration
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
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// <param name="migrationBuilder">
        /// The <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder" /> that will
        /// build the operations.
        /// </param>
        /// <seealso cref="M:Microsoft.EntityFrameworkCore.Migrations.Migration.Up(MigrationBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "S2Cell3",
                table: "Airports",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "S2Cell4",
                table: "Airports",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "S2Cell5",
                table: "Airports",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "S2Cell6",
                table: "Airports",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "S2Cell7",
                table: "Airports",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "S2Cell8",
                table: "Airports",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "S2Cell9",
                table: "Airports",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

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
        /// sushi.at, 08/12/2021.
        /// </remarks>
        /// <param name="migrationBuilder">
        /// The <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder" /> that will
        /// build the operations.
        /// </param>
        /// <seealso cref="M:Microsoft.EntityFrameworkCore.Migrations.Migration.Down(MigrationBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Airports_S2Cell3",
                table: "Airports");

            migrationBuilder.DropIndex(
                name: "IX_Airports_S2Cell4",
                table: "Airports");

            migrationBuilder.DropIndex(
                name: "IX_Airports_S2Cell5",
                table: "Airports");

            migrationBuilder.DropIndex(
                name: "IX_Airports_S2Cell6",
                table: "Airports");

            migrationBuilder.DropIndex(
                name: "IX_Airports_S2Cell7",
                table: "Airports");

            migrationBuilder.DropIndex(
                name: "IX_Airports_S2Cell8",
                table: "Airports");

            migrationBuilder.DropIndex(
                name: "IX_Airports_S2Cell9",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "S2Cell3",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "S2Cell4",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "S2Cell5",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "S2Cell6",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "S2Cell7",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "S2Cell8",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "S2Cell9",
                table: "Airports");
        }
    }
}
