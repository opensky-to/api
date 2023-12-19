using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenSky.API.Migrations
{
    /// <inheritdoc />
    public partial class NotificationMarkForDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "MarkedForDeletion",
                table: "Notifications",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarkedForDeletion",
                table: "Notifications");
        }
    }
}
