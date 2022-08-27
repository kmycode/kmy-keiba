using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddHorseRaceCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "GoalOrder",
                table: "RaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "RaceCount",
                table: "RaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "TimeDifference",
                table: "RaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoalOrder",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "RaceCount",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "TimeDifference",
                table: "RaceHorses");
        }
    }
}
