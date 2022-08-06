using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddRiderTrainer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestRaceHorses");

            migrationBuilder.DropTable(
                name: "TestRaces");

            migrationBuilder.DropColumn(
                name: "Corner1LapTime",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner1LapTimeValue",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner2LapTime",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner2LapTimeValue",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner3LapTime",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner3LapTimeValue",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner4LapTime",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner4LapTimeValue",
                table: "Races");

            migrationBuilder.AddColumn<byte[]>(
                name: "LapTimes",
                table: "Races",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "Riders",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Belongs = table.Column<short>(type: "INTEGER", nullable: false),
                    Sex = table.Column<short>(type: "INTEGER", nullable: false),
                    Issued = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Deleted = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Born = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 56, nullable: false),
                    TrainerCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    FirstRideRaceKey = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FirstRideRaceKeySteeplechase = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FirstWinRaceKey = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FirstWinRaceKeySteeplechase = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    From = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Riders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trainers",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Belongs = table.Column<short>(type: "INTEGER", nullable: false),
                    Sex = table.Column<short>(type: "INTEGER", nullable: false),
                    Issued = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Deleted = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Born = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 56, nullable: false),
                    From = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trainers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Riders");

            migrationBuilder.DropTable(
                name: "Trainers");

            migrationBuilder.DropColumn(
                name: "LapTimes",
                table: "Races");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Corner1LapTime",
                table: "Races",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<short>(
                name: "Corner1LapTimeValue",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Corner2LapTime",
                table: "Races",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<short>(
                name: "Corner2LapTimeValue",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Corner3LapTime",
                table: "Races",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<short>(
                name: "Corner3LapTimeValue",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Corner4LapTime",
                table: "Races",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<short>(
                name: "Corner4LapTimeValue",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: "TestRaceHorses",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AbnormalResult = table.Column<short>(type: "INTEGER", nullable: false),
                    AfterThirdHalongTimeValue = table.Column<short>(type: "INTEGER", nullable: false),
                    Age = table.Column<short>(type: "INTEGER", nullable: false),
                    Color = table.Column<short>(type: "INTEGER", nullable: false),
                    Course = table.Column<short>(type: "INTEGER", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false),
                    FailedType = table.Column<short>(type: "INTEGER", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Number = table.Column<short>(type: "INTEGER", nullable: false),
                    PassDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RaceKey = table.Column<string>(type: "TEXT", nullable: false),
                    ResultLength1 = table.Column<short>(type: "INTEGER", nullable: false),
                    ResultLength2 = table.Column<short>(type: "INTEGER", nullable: false),
                    ResultLength3 = table.Column<short>(type: "INTEGER", nullable: false),
                    ResultOrder = table.Column<short>(type: "INTEGER", nullable: false),
                    RiderCode = table.Column<string>(type: "TEXT", nullable: false),
                    RiderName = table.Column<string>(type: "TEXT", nullable: false),
                    RiderWeight = table.Column<short>(type: "INTEGER", nullable: false),
                    Sex = table.Column<short>(type: "INTEGER", nullable: false),
                    TestResult = table.Column<short>(type: "INTEGER", nullable: false),
                    TestType = table.Column<short>(type: "INTEGER", nullable: false),
                    TrainerCode = table.Column<string>(type: "TEXT", nullable: false),
                    TrainerName = table.Column<string>(type: "TEXT", nullable: false),
                    Weight = table.Column<short>(type: "INTEGER", nullable: false),
                    WeightDiff = table.Column<short>(type: "INTEGER", nullable: false)
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
                    Course = table.Column<short>(type: "INTEGER", nullable: false),
                    CourseRaceNumber = table.Column<short>(type: "INTEGER", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false),
                    Distance = table.Column<short>(type: "INTEGER", nullable: false),
                    HorsesCount = table.Column<short>(type: "INTEGER", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TrackCondition = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackCornerDirection = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackGround = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackOption = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackType = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackWeather = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestRaces", x => x.Id);
                });
        }
    }
}
