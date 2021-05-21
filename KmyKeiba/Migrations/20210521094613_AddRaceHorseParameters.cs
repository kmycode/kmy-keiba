using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KmyKeiba.Migrations
{
    public partial class AddRaceHorseParameters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name6Chars",
                table: "Races",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "FirstCornerOrder",
                table: "RaceHorses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FourthCornerOrder",
                table: "RaceHorses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Odds",
                table: "RaceHorses",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ResultTime",
                table: "RaceHorses",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "RiderCode",
                table: "RaceHorses",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RiderName",
                table: "RaceHorses",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "SecondCornerOrder",
                table: "RaceHorses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ThirdCornerOrder",
                table: "RaceHorses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name6Chars",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "FirstCornerOrder",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "FourthCornerOrder",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "Odds",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "ResultTime",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "RiderCode",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "RiderName",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "SecondCornerOrder",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "ThirdCornerOrder",
                table: "RaceHorses");
        }
    }
}
