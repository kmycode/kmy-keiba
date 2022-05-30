using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class UpdateStandardTimes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "UntilA3FAverage",
                table: "RaceStandardTimes",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "UntilA3FDeviation",
                table: "RaceStandardTimes",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "UntilA3FMedian",
                table: "RaceStandardTimes",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UntilA3FAverage",
                table: "RaceStandardTimes");

            migrationBuilder.DropColumn(
                name: "UntilA3FDeviation",
                table: "RaceStandardTimes");

            migrationBuilder.DropColumn(
                name: "UntilA3FMedian",
                table: "RaceStandardTimes");
        }
    }
}
