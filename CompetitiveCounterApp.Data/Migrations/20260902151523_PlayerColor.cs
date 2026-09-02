using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompetitiveCounterApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class PlayerColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorHex",
                table: "Players");

            migrationBuilder.AddColumn<string>(
                name: "ColorDark",
                table: "Players",
                type: "TEXT",
                nullable: false,
                defaultValue: "#EF9A9A");

            migrationBuilder.AddColumn<string>(
                name: "ColorLight",
                table: "Players",
                type: "TEXT",
                nullable: false,
                defaultValue: "#C62828");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorDark",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ColorLight",
                table: "Players");

            migrationBuilder.AddColumn<string>(
                name: "ColorHex",
                table: "Players",
                type: "TEXT",
                nullable: false,
                defaultValue: "#FF6B6B");
        }
    }
}
