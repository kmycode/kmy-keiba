using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalysisTableScriptIdToRow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "AnalysisTableScriptId",
                table: "AnalysisTableRows",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnalysisTableScriptId",
                table: "AnalysisTableRows");
        }
    }
}
