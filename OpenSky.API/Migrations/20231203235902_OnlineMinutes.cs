using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Online minutes migration.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class OnlineMinutes : Migration
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
                name: "OnlineNetworkConnectedMinutes",
                table: "Flights",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
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
            migrationBuilder.DropColumn(
                name: "OnlineNetworkConnectedMinutes",
                table: "Flights");
        }
    }
}
