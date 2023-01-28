using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class renamemovietoperk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PerkPerkCategory_Movies_PerksId",
                table: "PerkPerkCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Movies",
                table: "Movies");

            migrationBuilder.RenameTable(
                name: "Movies",
                newName: "Perks");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Perks",
                table: "Perks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PerkPerkCategory_Perks_PerksId",
                table: "PerkPerkCategory",
                column: "PerksId",
                principalTable: "Perks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PerkPerkCategory_Perks_PerksId",
                table: "PerkPerkCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Perks",
                table: "Perks");

            migrationBuilder.RenameTable(
                name: "Perks",
                newName: "Movies");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Movies",
                table: "Movies",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PerkPerkCategory_Movies_PerksId",
                table: "PerkPerkCategory",
                column: "PerksId",
                principalTable: "Movies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
