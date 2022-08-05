using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class ConvertTimeSpanValues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "Corner1LapTimeValue",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Corner2LapTimeValue",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Corner3LapTimeValue",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Corner4LapTimeValue",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "AfterThirdHalongTimeValue",
                table: "RaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "ResultTimeValue",
                table: "RaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Corner1LapTimeValue",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner2LapTimeValue",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner3LapTimeValue",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner4LapTimeValue",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "AfterThirdHalongTimeValue",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "ResultTimeValue",
                table: "RaceHorses");
        }
    }
}
