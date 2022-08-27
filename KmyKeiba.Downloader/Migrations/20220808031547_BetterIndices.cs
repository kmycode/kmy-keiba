using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class BetterIndices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Races_StartTime_Key_Course",
                table: "Races");

            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_RaceKey_Key_RiderCode_TrainerCode_RaceCount_RaceCountWithinRunning_RaceCountWithinRunningCompletely",
                table: "RaceHorses");

            migrationBuilder.CreateIndex(
                name: "IX_Races_Course",
                table: "Races",
                column: "Course");

            migrationBuilder.CreateIndex(
                name: "IX_Races_Key",
                table: "Races",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_Races_StartTime",
                table: "Races",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_Key_RaceCount_RaceCountWithinRunning_RaceCountWithinRunningCompletely",
                table: "RaceHorses",
                columns: new[] { "Key", "RaceCount", "RaceCountWithinRunning", "RaceCountWithinRunningCompletely" });

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_RaceKey_Key",
                table: "RaceHorses",
                columns: new[] { "RaceKey", "Key" });

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_RiderCode",
                table: "RaceHorses",
                column: "RiderCode");

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_TrainerCode",
                table: "RaceHorses",
                column: "TrainerCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Races_Course",
                table: "Races");

            migrationBuilder.DropIndex(
                name: "IX_Races_Key",
                table: "Races");

            migrationBuilder.DropIndex(
                name: "IX_Races_StartTime",
                table: "Races");

            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_Key_RaceCount_RaceCountWithinRunning_RaceCountWithinRunningCompletely",
                table: "RaceHorses");

            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_RaceKey_Key",
                table: "RaceHorses");

            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_RiderCode",
                table: "RaceHorses");

            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_TrainerCode",
                table: "RaceHorses");

            migrationBuilder.CreateIndex(
                name: "IX_Races_StartTime_Key_Course",
                table: "Races",
                columns: new[] { "StartTime", "Key", "Course" });

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_RaceKey_Key_RiderCode_TrainerCode_RaceCount_RaceCountWithinRunning_RaceCountWithinRunningCompletely",
                table: "RaceHorses",
                columns: new[] { "RaceKey", "Key", "RiderCode", "TrainerCode", "RaceCount", "RaceCountWithinRunning", "RaceCountWithinRunningCompletely" });
        }
    }
}
