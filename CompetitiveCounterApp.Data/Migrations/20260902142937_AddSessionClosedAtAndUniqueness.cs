using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompetitiveCounterApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionClosedAtAndUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_GameID",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_SessionPlayers_SessionID",
                table: "SessionPlayers");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "Sessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_GameID_Active",
                table: "Sessions",
                column: "GameID",
                unique: true,
                filter: "\"ClosedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPlayers_SessionID_PlayerID",
                table: "SessionPlayers",
                columns: new[] { "SessionID", "PlayerID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_GameID_Active",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_SessionPlayers_SessionID_PlayerID",
                table: "SessionPlayers");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "Sessions");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_GameID",
                table: "Sessions",
                column: "GameID");

            migrationBuilder.CreateIndex(
                name: "IX_SessionPlayers_SessionID",
                table: "SessionPlayers",
                column: "SessionID");
        }
    }
}
