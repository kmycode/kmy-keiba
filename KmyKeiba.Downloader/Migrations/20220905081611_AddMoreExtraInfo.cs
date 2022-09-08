using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddMoreExtraInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "ExtraDataState",
                table: "RaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "ExtraDataVersion",
                table: "RaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "After3HaronOrder",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BaseTime",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BaseTimeAs3Haron",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Before3HaronTimeFixed",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CornerAloneCount",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CornerInsideCount",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CornerMiddleCount",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CornerOrderDiff2",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CornerOrderDiff3",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CornerOrderDiff4",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CornerOrderDiffGoal",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CornerOutsideCount",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Pci",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Pci3",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Rpci",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraDataState",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "ExtraDataVersion",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "After3HaronOrder",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "BaseTime",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "BaseTimeAs3Haron",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "Before3HaronTimeFixed",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CornerAloneCount",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CornerInsideCount",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CornerMiddleCount",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CornerOrderDiff2",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CornerOrderDiff3",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CornerOrderDiff4",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CornerOrderDiffGoal",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CornerOutsideCount",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "Pci",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "Pci3",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "Rpci",
                table: "RaceHorseExtras");
        }
    }
}
