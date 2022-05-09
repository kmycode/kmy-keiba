using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddMasterData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RaceHorseAnalysis");

            migrationBuilder.AddColumn<bool>(
                name: "IsRunningStyleSetManually",
                table: "RaceHorses",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RaceStandardTimes",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Course = table.Column<short>(type: "smallint", nullable: false),
                    SampleStartTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SampleEndTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SampleCount = table.Column<int>(type: "int", nullable: false),
                    CornerDirection = table.Column<short>(type: "smallint", nullable: false),
                    TrackOption = table.Column<short>(type: "smallint", nullable: false),
                    Ground = table.Column<short>(type: "smallint", nullable: false),
                    Weather = table.Column<short>(type: "smallint", nullable: false),
                    Condition = table.Column<short>(type: "smallint", nullable: false),
                    TrackType = table.Column<short>(type: "smallint", nullable: false),
                    Distance = table.Column<short>(type: "smallint", nullable: false),
                    Average = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Median = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Deviation = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Version = table.Column<ushort>(type: "smallint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceStandardTimes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RaceStandardTimes");

            migrationBuilder.DropColumn(
                name: "IsRunningStyleSetManually",
                table: "RaceHorses");

            migrationBuilder.CreateTable(
                name: "RaceHorseAnalysis",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AnalysisVersion = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    BreakthroughPoint = table.Column<short>(type: "smallint", nullable: false),
                    CanceledStallPosition = table.Column<short>(type: "smallint", nullable: false),
                    CanceledStallRecoveredPosition = table.Column<short>(type: "smallint", nullable: false),
                    CornerPoint = table.Column<short>(type: "smallint", nullable: false),
                    DownhillPoint = table.Column<short>(type: "smallint", nullable: false),
                    GutsPoint = table.Column<short>(type: "smallint", nullable: false),
                    Key = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RaceKey = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RunningStyle = table.Column<short>(type: "smallint", nullable: false),
                    RunningStyleResult = table.Column<short>(type: "smallint", nullable: false),
                    SpeedPoint = table.Column<short>(type: "smallint", nullable: false),
                    StallPosition = table.Column<short>(type: "smallint", nullable: false),
                    StraightPoint = table.Column<short>(type: "smallint", nullable: false),
                    UphillPoint = table.Column<short>(type: "smallint", nullable: false)
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
    }
}
