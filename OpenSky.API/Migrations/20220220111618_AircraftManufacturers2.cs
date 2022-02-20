using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Aircraft manufacturers migration - step 1/2.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class AircraftManufacturers2 : Migration
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
                name: "FK_AircraftTypes_AircraftManufacturers_AircraftManufacturerID",
                table: "AircraftTypes");

            migrationBuilder.DropIndex(
                name: "IX_AircraftTypes_AircraftManufacturerID",
                table: "AircraftTypes");

            migrationBuilder.DropColumn(
                name: "AircraftManufacturerID",
                table: "AircraftTypes");

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerID",
                table: "AircraftTypes",
                type: "varchar(5)",
                maxLength: 5,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftTypes_ManufacturerID",
                table: "AircraftTypes",
                column: "ManufacturerID");

            migrationBuilder.AddForeignKey(
                name: "FK_AircraftTypes_AircraftManufacturers_ManufacturerID",
                table: "AircraftTypes",
                column: "ManufacturerID",
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
                name: "FK_AircraftTypes_AircraftManufacturers_ManufacturerID",
                table: "AircraftTypes");

            migrationBuilder.DropIndex(
                name: "IX_AircraftTypes_ManufacturerID",
                table: "AircraftTypes");

            migrationBuilder.DropColumn(
                name: "ManufacturerID",
                table: "AircraftTypes");

            migrationBuilder.AddColumn<string>(
                name: "AircraftManufacturerID",
                table: "AircraftTypes",
                type: "varchar(5)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AircraftTypes_AircraftManufacturerID",
                table: "AircraftTypes",
                column: "AircraftManufacturerID");

            migrationBuilder.AddForeignKey(
                name: "FK_AircraftTypes_AircraftManufacturers_AircraftManufacturerID",
                table: "AircraftTypes",
                column: "AircraftManufacturerID",
                principalTable: "AircraftManufacturers",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
