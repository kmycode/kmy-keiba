using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class UpdateHorseIndexRaceCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_RaceKey_Key_RiderCode_TrainerCode_CourseCode",
                table: "RaceHorses");

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_RaceKey_Key_RiderCode_TrainerCode_RaceCount",
                table: "RaceHorses",
                columns: new[] { "RaceKey", "Key", "RiderCode", "TrainerCode", "RaceCount" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_RaceKey_Key_RiderCode_TrainerCode_RaceCount",
                table: "RaceHorses");

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_RaceKey_Key_RiderCode_TrainerCode_CourseCode",
                table: "RaceHorses",
                columns: new[] { "RaceKey", "Key", "RiderCode", "TrainerCode", "CourseCode" });
        }
    }
}
