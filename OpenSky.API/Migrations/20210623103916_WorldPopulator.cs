using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// World aircraft populator migration.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class WorldPopulator : Migration
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
        /// sushi.at, 26/06/2021.
        /// </remarks>
        /// <param name="migrationBuilder">
        /// The <see cref="T:Microsoft.EntityFrameworkCore.Migrations.MigrationBuilder" /> that will
        /// build the operations.
        /// </param>
        /// <seealso cref="M:Microsoft.EntityFrameworkCore.Migrations.Migration.Up(MigrationBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Country",
                table: "Airports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HasBeenPopulated",
                table: "Airports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PurchasePrice",
                table: "Aircraft",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RentPrice",
                table: "Aircraft",
                type: "int",
                nullable: true);
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
        /// sushi.at, 26/06/2021.
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
                name: "Country",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "HasBeenPopulated",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "PurchasePrice",
                table: "Aircraft");

            migrationBuilder.DropColumn(
                name: "RentPrice",
                table: "Aircraft");
        }
    }
}
