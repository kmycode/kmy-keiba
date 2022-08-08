using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddRaceCountsIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_RaceKey_Key_RiderCode_TrainerCode_RaceCount",
                table: "RaceHorses");

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_RaceKey_Key_RiderCode_TrainerCode_RaceCount_RaceCountWithinRunning_RaceCountWithinRunningCompletely",
                table: "RaceHorses",
                columns: new[] { "RaceKey", "Key", "RiderCode", "TrainerCode", "RaceCount", "RaceCountWithinRunning", "RaceCountWithinRunningCompletely" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_RaceKey_Key_RiderCode_TrainerCode_RaceCount_RaceCountWithinRunning_RaceCountWithinRunningCompletely",
                table: "RaceHorses");

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_RaceKey_Key_RiderCode_TrainerCode_RaceCount",
                table: "RaceHorses",
                columns: new[] { "RaceKey", "Key", "RiderCode", "TrainerCode", "RaceCount" });
        }
    }
}
