using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddValueScript : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ValueScript",
                table: "AnalysisTableRows",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_HorseMarks_RaceKey",
                table: "HorseMarks",
                column: "RaceKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HorseMarks_RaceKey",
                table: "HorseMarks");

            migrationBuilder.DropColumn(
                name: "ValueScript",
                table: "AnalysisTableRows");
        }
    }
}
