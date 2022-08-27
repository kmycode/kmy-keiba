using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddRaceCounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "RaceCountWithinRunning",
                table: "RaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "RaceCountWithinRunningCompletely",
                table: "RaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RaceCountWithinRunning",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "RaceCountWithinRunningCompletely",
                table: "RaceHorses");
        }
    }
}
