using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddRaceDetailInfos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_Key_RaceCount_RaceCountWithinRunning_RaceCountWithinRunningCompletely",
                table: "RaceHorses");

            migrationBuilder.AddColumn<short>(
                name: "Area",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Cross",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Nichiji",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "RiderWeight",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Sex",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "RaceCountAfterLastRest",
                table: "RaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_Key_RaceCount_RaceCountWithinRunning_RaceCountWithinRunningCompletely_RaceCountAfterLastRest",
                table: "RaceHorses",
                columns: new[] { "Key", "RaceCount", "RaceCountWithinRunning", "RaceCountWithinRunningCompletely", "RaceCountAfterLastRest" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_Key_RaceCount_RaceCountWithinRunning_RaceCountWithinRunningCompletely_RaceCountAfterLastRest",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "Area",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Cross",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Nichiji",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "RiderWeight",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "RaceCountAfterLastRest",
                table: "RaceHorses");

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_Key_RaceCount_RaceCountWithinRunning_RaceCountWithinRunningCompletely",
                table: "RaceHorses",
                columns: new[] { "Key", "RaceCount", "RaceCountWithinRunning", "RaceCountWithinRunningCompletely" });
        }
    }
}
