using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KmyKeiba.Migrations
{
    public partial class AddHalongTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "AfterThirdHalongTime",
                table: "RaceHorses",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "AfterThirdHalongTimeOrder",
                table: "RaceHorses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AfterThirdHalongTime",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "AfterThirdHalongTimeOrder",
                table: "RaceHorses");
        }
    }
}
