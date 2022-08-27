using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddTableRowDefaultValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AlternativeValueIfEmpty",
                table: "AnalysisTableRows",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_JrdbRaceHorses_RaceKey_Key",
                table: "JrdbRaceHorses",
                columns: new[] { "RaceKey", "Key" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JrdbRaceHorses_RaceKey_Key",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "AlternativeValueIfEmpty",
                table: "AnalysisTableRows");
        }
    }
}
