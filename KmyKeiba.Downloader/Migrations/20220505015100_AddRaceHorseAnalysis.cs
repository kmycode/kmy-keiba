using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddRaceHorseAnalysis : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalcedRunningStyle",
                table: "RaceHorses");

            migrationBuilder.CreateTable(
                name: "RaceHorseAnalysis",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RaceKey = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RunningStyle = table.Column<short>(type: "smallint", nullable: false),
                    RunningStyleResult = table.Column<short>(type: "smallint", nullable: false),
                    StallPosition = table.Column<short>(type: "smallint", nullable: false),
                    CanceledStallPosition = table.Column<short>(type: "smallint", nullable: false),
                    CanceledStallRecoveredPosition = table.Column<short>(type: "smallint", nullable: false),
                    SpeedPoint = table.Column<short>(type: "smallint", nullable: false),
                    BreakthroughPoint = table.Column<short>(type: "smallint", nullable: false),
                    GutsPoint = table.Column<short>(type: "smallint", nullable: false),
                    StraightPoint = table.Column<short>(type: "smallint", nullable: false),
                    CornerPoint = table.Column<short>(type: "smallint", nullable: false),
                    UphillPoint = table.Column<short>(type: "smallint", nullable: false),
                    DownhillPoint = table.Column<short>(type: "smallint", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AnalysisVersion = table.Column<ushort>(type: "smallint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceHorseAnalysis", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorseAnalysis_RaceKey_Key",
                table: "RaceHorseAnalysis",
                columns: new[] { "RaceKey", "Key" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RaceHorseAnalysis");

            migrationBuilder.AddColumn<short>(
                name: "CalcedRunningStyle",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }
    }
}
