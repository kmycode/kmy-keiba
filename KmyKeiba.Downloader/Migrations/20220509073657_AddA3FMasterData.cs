using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddA3FMasterData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "A3FAverage",
                table: "RaceStandardTimes",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "A3FDeviation",
                table: "RaceStandardTimes",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "A3FMedian",
                table: "RaceStandardTimes",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_RaceStandardTimes_Course",
                table: "RaceStandardTimes",
                column: "Course");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RaceStandardTimes_Course",
                table: "RaceStandardTimes");

            migrationBuilder.DropColumn(
                name: "A3FAverage",
                table: "RaceStandardTimes");

            migrationBuilder.DropColumn(
                name: "A3FDeviation",
                table: "RaceStandardTimes");

            migrationBuilder.DropColumn(
                name: "A3FMedian",
                table: "RaceStandardTimes");
        }
    }
}
