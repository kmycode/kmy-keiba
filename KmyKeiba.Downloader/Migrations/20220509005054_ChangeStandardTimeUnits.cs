using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class ChangeStandardTimeUnits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Median",
                table: "RaceStandardTimes",
                type: "double",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time(6)");

            migrationBuilder.AlterColumn<double>(
                name: "Deviation",
                table: "RaceStandardTimes",
                type: "double",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time(6)");

            migrationBuilder.AlterColumn<double>(
                name: "Average",
                table: "RaceStandardTimes",
                type: "double",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time(6)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "Median",
                table: "RaceStandardTimes",
                type: "time(6)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "Deviation",
                table: "RaceStandardTimes",
                type: "time(6)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "Average",
                table: "RaceStandardTimes",
                type: "time(6)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");
        }
    }
}
