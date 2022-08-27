using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddPciMasterData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Pci3Average",
                table: "RaceStandardTimes",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Pci3Deviation",
                table: "RaceStandardTimes",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Pci3Median",
                table: "RaceStandardTimes",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PciAverage",
                table: "RaceStandardTimes",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PciDeviation",
                table: "RaceStandardTimes",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "PciMedian",
                table: "RaceStandardTimes",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RpciAverage",
                table: "RaceStandardTimes",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RpciDeviation",
                table: "RaceStandardTimes",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RpciMedian",
                table: "RaceStandardTimes",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pci3Average",
                table: "RaceStandardTimes");

            migrationBuilder.DropColumn(
                name: "Pci3Deviation",
                table: "RaceStandardTimes");

            migrationBuilder.DropColumn(
                name: "Pci3Median",
                table: "RaceStandardTimes");

            migrationBuilder.DropColumn(
                name: "PciAverage",
                table: "RaceStandardTimes");

            migrationBuilder.DropColumn(
                name: "PciDeviation",
                table: "RaceStandardTimes");

            migrationBuilder.DropColumn(
                name: "PciMedian",
                table: "RaceStandardTimes");

            migrationBuilder.DropColumn(
                name: "RpciAverage",
                table: "RaceStandardTimes");

            migrationBuilder.DropColumn(
                name: "RpciDeviation",
                table: "RaceStandardTimes");

            migrationBuilder.DropColumn(
                name: "RpciMedian",
                table: "RaceStandardTimes");
        }
    }
}
