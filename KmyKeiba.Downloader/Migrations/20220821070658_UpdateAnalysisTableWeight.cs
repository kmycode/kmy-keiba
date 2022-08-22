using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class UpdateAnalysisTableWeight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Config",
                table: "AnalysisTableWeights");

            migrationBuilder.RenameColumn(
                name: "FilterId",
                table: "AnalysisTableRows",
                newName: "ParentRowId");

            migrationBuilder.AddColumn<uint>(
                name: "ExternalNumberId",
                table: "AnalysisTableRows",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "AnalysisTableWeightRows",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WeightId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    FinderConfig = table.Column<string>(type: "TEXT", nullable: false),
                    Weight = table.Column<double>(type: "REAL", nullable: false),
                    Behavior = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<ushort>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisTableWeightRows", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisTableWeightRows");

            migrationBuilder.DropColumn(
                name: "ExternalNumberId",
                table: "AnalysisTableRows");

            migrationBuilder.RenameColumn(
                name: "ParentRowId",
                table: "AnalysisTableRows",
                newName: "FilterId");

            migrationBuilder.AddColumn<string>(
                name: "Config",
                table: "AnalysisTableWeights",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
