using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompetitiveCounterApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class PlayerIcon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Players",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Players");
        }
    }
}
