using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddMultipleWeights : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "RequestedSize",
                table: "AnalysisTableRows",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<uint>(
                name: "Weight2Id",
                table: "AnalysisTableRows",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "Weight3Id",
                table: "AnalysisTableRows",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalNumbers_RaceKey",
                table: "ExternalNumbers",
                column: "RaceKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExternalNumbers_RaceKey",
                table: "ExternalNumbers");

            migrationBuilder.DropColumn(
                name: "RequestedSize",
                table: "AnalysisTableRows");

            migrationBuilder.DropColumn(
                name: "Weight2Id",
                table: "AnalysisTableRows");

            migrationBuilder.DropColumn(
                name: "Weight3Id",
                table: "AnalysisTableRows");
        }
    }
}
