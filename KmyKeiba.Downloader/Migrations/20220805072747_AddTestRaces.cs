using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddTestRaces : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestRaceHorses",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    PassDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RaceKey = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Age = table.Column<short>(type: "INTEGER", nullable: false),
                    Sex = table.Column<short>(type: "INTEGER", nullable: false),
                    Color = table.Column<short>(type: "INTEGER", nullable: false),
                    Number = table.Column<short>(type: "INTEGER", nullable: false),
                    Course = table.Column<short>(type: "INTEGER", nullable: false),
                    ResultOrder = table.Column<short>(type: "INTEGER", nullable: false),
                    AbnormalResult = table.Column<short>(type: "INTEGER", nullable: false),
                    ResultLength1 = table.Column<short>(type: "INTEGER", nullable: false),
                    ResultLength2 = table.Column<short>(type: "INTEGER", nullable: false),
                    ResultLength3 = table.Column<short>(type: "INTEGER", nullable: false),
                    RiderCode = table.Column<string>(type: "TEXT", nullable: false),
                    RiderName = table.Column<string>(type: "TEXT", nullable: false),
                    RiderWeight = table.Column<short>(type: "INTEGER", nullable: false),
                    TrainerCode = table.Column<string>(type: "TEXT", nullable: false),
                    TrainerName = table.Column<string>(type: "TEXT", nullable: false),
                    Weight = table.Column<short>(type: "INTEGER", nullable: false),
                    WeightDiff = table.Column<short>(type: "INTEGER", nullable: false),
                    TestType = table.Column<short>(type: "INTEGER", nullable: false),
                    TestResult = table.Column<short>(type: "INTEGER", nullable: false),
                    FailedType = table.Column<short>(type: "INTEGER", nullable: false),
                    AfterThirdHalongTimeValue = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestRaceHorses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestRaces",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Course = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackGround = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackCornerDirection = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackType = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackOption = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackWeather = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackCondition = table.Column<short>(type: "INTEGER", nullable: false),
                    Distance = table.Column<short>(type: "INTEGER", nullable: false),
                    CourseRaceNumber = table.Column<short>(type: "INTEGER", nullable: false),
                    HorsesCount = table.Column<short>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestRaces", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestRaceHorses");

            migrationBuilder.DropTable(
                name: "TestRaces");
        }
    }
}
