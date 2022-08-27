using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class MoveBaseWeight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseWeight",
                table: "AnalysisTableWeights");

            migrationBuilder.AddColumn<double>(
                name: "BaseWeight",
                table: "AnalysisTableRows",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseWeight",
                table: "AnalysisTableRows");

            migrationBuilder.AddColumn<double>(
                name: "BaseWeight",
                table: "AnalysisTableWeights",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
