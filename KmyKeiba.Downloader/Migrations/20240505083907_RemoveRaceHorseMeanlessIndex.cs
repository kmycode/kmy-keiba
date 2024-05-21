using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRaceHorseMeanlessIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_Key_RaceCount_RaceCountWithinRunning_RaceCountWithinRunningCompletely_RaceCountAfterLastRest",
                table: "RaceHorses");

            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_RaceKey_Key",
                table: "RaceHorses");

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_Key",
                table: "RaceHorses",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_RaceKey",
                table: "RaceHorses",
                column: "RaceKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_Key",
                table: "RaceHorses");

            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_RaceKey",
                table: "RaceHorses");

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_Key_RaceCount_RaceCountWithinRunning_RaceCountWithinRunningCompletely_RaceCountAfterLastRest",
                table: "RaceHorses",
                columns: new[] { "Key", "RaceCount", "RaceCountWithinRunning", "RaceCountWithinRunningCompletely", "RaceCountAfterLastRest" });

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_RaceKey_Key",
                table: "RaceHorses",
                columns: new[] { "RaceKey", "Key" });
        }
    }
}
