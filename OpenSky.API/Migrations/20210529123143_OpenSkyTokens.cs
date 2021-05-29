using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenSky.API.Migrations
{
    public partial class OpenSkyTokens : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpenSkyTokens",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Expiry = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_OpenSkyTokens_UserID",
                table: "OpenSkyTokens",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpenSkyTokens");
        }
    }
}
