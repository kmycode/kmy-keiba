using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ExactaOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorseNumber1 = table.Column<short>(type: "smallint", nullable: false),
                    HorseNumber2 = table.Column<short>(type: "smallint", nullable: false),
                    Odds = table.Column<float>(type: "float", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExactaOdds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FrameNumberOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Frame1 = table.Column<short>(type: "smallint", nullable: false),
                    Frame2 = table.Column<short>(type: "smallint", nullable: false),
                    Odds = table.Column<float>(type: "float", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrameNumberOdds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LearningDataCaches",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorseName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CacheVersion = table.Column<int>(type: "int", nullable: false),
                    Cache = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningDataCaches", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuinellaOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorseNumber1 = table.Column<short>(type: "smallint", nullable: false),
                    HorseNumber2 = table.Column<short>(type: "smallint", nullable: false),
                    Odds = table.Column<float>(type: "float", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuinellaOdds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuinellaPlaceOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorseNumber1 = table.Column<short>(type: "smallint", nullable: false),
                    HorseNumber2 = table.Column<short>(type: "smallint", nullable: false),
                    PlaceOddsMin = table.Column<float>(type: "float", nullable: false),
                    PlaceOddsMax = table.Column<float>(type: "float", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuinellaPlaceOdds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RaceHorses",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Age = table.Column<short>(type: "smallint", nullable: false),
                    Sex = table.Column<short>(type: "smallint", nullable: false),
                    CourseCode = table.Column<short>(type: "smallint", nullable: false),
                    Mark = table.Column<short>(type: "smallint", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    FrameNumber = table.Column<int>(type: "int", nullable: false),
                    ResultOrder = table.Column<int>(type: "int", nullable: false),
                    AbnormalResult = table.Column<short>(type: "smallint", nullable: false),
                    Popular = table.Column<int>(type: "int", nullable: false),
                    ResultTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    FirstCornerOrder = table.Column<int>(type: "int", nullable: false),
                    SecondCornerOrder = table.Column<int>(type: "int", nullable: false),
                    ThirdCornerOrder = table.Column<int>(type: "int", nullable: false),
                    FourthCornerOrder = table.Column<int>(type: "int", nullable: false),
                    RiderCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RiderName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RiderWeight = table.Column<float>(type: "float", nullable: false),
                    TrainerCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrainerName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsBlinkers = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Weight = table.Column<short>(type: "smallint", nullable: false),
                    WeightDiff = table.Column<short>(type: "smallint", nullable: false),
                    Odds = table.Column<float>(type: "float", nullable: false),
                    PlaceOddsMin = table.Column<float>(type: "float", nullable: false),
                    PlaceOddsMax = table.Column<float>(type: "float", nullable: false),
                    AfterThirdHalongTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    AfterThirdHalongTimeOrder = table.Column<int>(type: "int", nullable: false),
                    RunningStyle = table.Column<short>(type: "smallint", nullable: false),
                    IsRunningStyleSetManually = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UniformFormat = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UniformFormatData = table.Column<byte[]>(type: "VARBINARY(8000)", maxLength: 8000, nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceHorses", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Races",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name6Chars = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Course = table.Column<short>(type: "smallint", nullable: false),
                    CourseType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrackGround = table.Column<short>(type: "smallint", nullable: false),
                    TrackCornerDirection = table.Column<short>(type: "smallint", nullable: false),
                    TrackType = table.Column<short>(type: "smallint", nullable: false),
                    TrackOption = table.Column<short>(type: "smallint", nullable: false),
                    TrackWeather = table.Column<short>(type: "smallint", nullable: false),
                    TrackCondition = table.Column<short>(type: "smallint", nullable: false),
                    Distance = table.Column<int>(type: "int", nullable: false),
                    CourseRaceNumber = table.Column<int>(type: "int", nullable: false),
                    SubjectName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    SubjectAge2 = table.Column<int>(type: "int", nullable: false),
                    SubjectAge3 = table.Column<int>(type: "int", nullable: false),
                    SubjectAge4 = table.Column<int>(type: "int", nullable: false),
                    SubjectAge5 = table.Column<int>(type: "int", nullable: false),
                    SubjectAgeYounger = table.Column<int>(type: "int", nullable: false),
                    HorsesCount = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CourseBaseTimeCache = table.Column<float>(type: "float", nullable: false),
                    CourseBaseTimeCacheVersion = table.Column<int>(type: "int", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Races", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Refunds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SingleNumber1 = table.Column<short>(type: "smallint", nullable: false),
                    SingleNumber1Money = table.Column<int>(type: "int", nullable: false),
                    SingleNumber2 = table.Column<short>(type: "smallint", nullable: false),
                    SingleNumber2Money = table.Column<int>(type: "int", nullable: false),
                    SingleNumber3 = table.Column<short>(type: "smallint", nullable: false),
                    SingleNumber3Money = table.Column<int>(type: "int", nullable: false),
                    PlaceNumber1 = table.Column<short>(type: "smallint", nullable: false),
                    PlaceNumber1Money = table.Column<int>(type: "int", nullable: false),
                    PlaceNumber2 = table.Column<short>(type: "smallint", nullable: false),
                    PlaceNumber2Money = table.Column<int>(type: "int", nullable: false),
                    PlaceNumber3 = table.Column<short>(type: "smallint", nullable: false),
                    PlaceNumber3Money = table.Column<int>(type: "int", nullable: false),
                    PlaceNumber4 = table.Column<short>(type: "smallint", nullable: false),
                    PlaceNumber4Money = table.Column<int>(type: "int", nullable: false),
                    PlaceNumber5 = table.Column<short>(type: "smallint", nullable: false),
                    PlaceNumber5Money = table.Column<int>(type: "int", nullable: false),
                    Frame1Number1 = table.Column<short>(type: "smallint", nullable: false),
                    Frame2Number1 = table.Column<short>(type: "smallint", nullable: false),
                    FrameNumber1Money = table.Column<int>(type: "int", nullable: false),
                    Frame1Number2 = table.Column<short>(type: "smallint", nullable: false),
                    Frame2Number2 = table.Column<short>(type: "smallint", nullable: false),
                    FrameNumber2Money = table.Column<int>(type: "int", nullable: false),
                    Frame1Number3 = table.Column<short>(type: "smallint", nullable: false),
                    Frame2Number3 = table.Column<short>(type: "smallint", nullable: false),
                    FrameNumber3Money = table.Column<int>(type: "int", nullable: false),
                    Quinella1Number1 = table.Column<short>(type: "smallint", nullable: false),
                    Quinella2Number1 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaNumber1Money = table.Column<int>(type: "int", nullable: false),
                    Quinella1Number2 = table.Column<short>(type: "smallint", nullable: false),
                    Quinella2Number2 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaNumber2Money = table.Column<int>(type: "int", nullable: false),
                    Quinella1Number3 = table.Column<short>(type: "smallint", nullable: false),
                    Quinella2Number3 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaNumber3Money = table.Column<int>(type: "int", nullable: false),
                    QuinellaPlace1Number1 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlace2Number1 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlaceNumber1Money = table.Column<int>(type: "int", nullable: false),
                    QuinellaPlace1Number2 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlace2Number2 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlaceNumber2Money = table.Column<int>(type: "int", nullable: false),
                    QuinellaPlace1Number3 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlace2Number3 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlaceNumber3Money = table.Column<int>(type: "int", nullable: false),
                    QuinellaPlace1Number4 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlace2Number4 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlaceNumber4Money = table.Column<int>(type: "int", nullable: false),
                    QuinellaPlace1Number5 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlace2Number5 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlaceNumber5Money = table.Column<int>(type: "int", nullable: false),
                    QuinellaPlace1Number6 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlace2Number6 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlaceNumber6Money = table.Column<int>(type: "int", nullable: false),
                    QuinellaPlace1Number7 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlace2Number7 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlaceNumber7Money = table.Column<int>(type: "int", nullable: false),
                    Exacta1Number1 = table.Column<short>(type: "smallint", nullable: false),
                    Exacta2Number1 = table.Column<short>(type: "smallint", nullable: false),
                    ExactaNumber1Money = table.Column<int>(type: "int", nullable: false),
                    Exacta1Number2 = table.Column<short>(type: "smallint", nullable: false),
                    Exacta2Number2 = table.Column<short>(type: "smallint", nullable: false),
                    ExactaNumber2Money = table.Column<int>(type: "int", nullable: false),
                    Exacta1Number3 = table.Column<short>(type: "smallint", nullable: false),
                    Exacta2Number3 = table.Column<short>(type: "smallint", nullable: false),
                    ExactaNumber3Money = table.Column<int>(type: "int", nullable: false),
                    Trio1Number1 = table.Column<short>(type: "smallint", nullable: false),
                    Trio2Number1 = table.Column<short>(type: "smallint", nullable: false),
                    Trio3Number1 = table.Column<short>(type: "smallint", nullable: false),
                    TrioNumber1Money = table.Column<int>(type: "int", nullable: false),
                    Trio1Number2 = table.Column<short>(type: "smallint", nullable: false),
                    Trio2Number2 = table.Column<short>(type: "smallint", nullable: false),
                    Trio3Number2 = table.Column<short>(type: "smallint", nullable: false),
                    TrioNumber2Money = table.Column<int>(type: "int", nullable: false),
                    Trio1Number3 = table.Column<short>(type: "smallint", nullable: false),
                    Trio2Number3 = table.Column<short>(type: "smallint", nullable: false),
                    Trio3Number3 = table.Column<short>(type: "smallint", nullable: false),
                    TrioNumber3Money = table.Column<int>(type: "int", nullable: false),
                    Trifecta1Number1 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta2Number1 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta3Number1 = table.Column<short>(type: "smallint", nullable: false),
                    TrifectaNumber1Money = table.Column<int>(type: "int", nullable: false),
                    Trifecta1Number2 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta2Number2 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta3Number2 = table.Column<short>(type: "smallint", nullable: false),
                    TrifectaNumber2Money = table.Column<int>(type: "int", nullable: false),
                    Trifecta1Number3 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta2Number3 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta3Number3 = table.Column<short>(type: "smallint", nullable: false),
                    TrifectaNumber3Money = table.Column<int>(type: "int", nullable: false),
                    Trifecta1Number4 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta2Number4 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta3Number4 = table.Column<short>(type: "smallint", nullable: false),
                    TrifectaNumber4Money = table.Column<int>(type: "int", nullable: false),
                    Trifecta1Number5 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta2Number5 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta3Number5 = table.Column<short>(type: "smallint", nullable: false),
                    TrifectaNumber5Money = table.Column<int>(type: "int", nullable: false),
                    Trifecta1Number6 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta2Number6 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta3Number6 = table.Column<short>(type: "smallint", nullable: false),
                    TrifectaNumber6Money = table.Column<int>(type: "int", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refunds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SingleOddsTimelines",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Odds1 = table.Column<float>(type: "float", nullable: false),
                    Odds2 = table.Column<float>(type: "float", nullable: false),
                    Odds3 = table.Column<float>(type: "float", nullable: false),
                    Odds4 = table.Column<float>(type: "float", nullable: false),
                    Odds5 = table.Column<float>(type: "float", nullable: false),
                    Odds6 = table.Column<float>(type: "float", nullable: false),
                    Odds7 = table.Column<float>(type: "float", nullable: false),
                    Odds8 = table.Column<float>(type: "float", nullable: false),
                    Odds9 = table.Column<float>(type: "float", nullable: false),
                    Odds10 = table.Column<float>(type: "float", nullable: false),
                    Odds11 = table.Column<float>(type: "float", nullable: false),
                    Odds12 = table.Column<float>(type: "float", nullable: false),
                    Odds13 = table.Column<float>(type: "float", nullable: false),
                    Odds14 = table.Column<float>(type: "float", nullable: false),
                    Odds15 = table.Column<float>(type: "float", nullable: false),
                    Odds16 = table.Column<float>(type: "float", nullable: false),
                    Odds17 = table.Column<float>(type: "float", nullable: false),
                    Odds18 = table.Column<float>(type: "float", nullable: false),
                    Odds19 = table.Column<float>(type: "float", nullable: false),
                    Odds20 = table.Column<float>(type: "float", nullable: false),
                    Odds21 = table.Column<float>(type: "float", nullable: false),
                    Odds22 = table.Column<float>(type: "float", nullable: false),
                    Odds23 = table.Column<float>(type: "float", nullable: false),
                    Odds24 = table.Column<float>(type: "float", nullable: false),
                    Odds25 = table.Column<float>(type: "float", nullable: false),
                    Odds26 = table.Column<float>(type: "float", nullable: false),
                    Odds27 = table.Column<float>(type: "float", nullable: false),
                    Odds28 = table.Column<float>(type: "float", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleOddsTimelines", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SystemData",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemData", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Trainings",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HorseKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Center = table.Column<short>(type: "smallint", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FirstLapTime = table.Column<float>(type: "float", nullable: false),
                    SecondLapTime = table.Column<float>(type: "float", nullable: false),
                    ThirdLapTime = table.Column<float>(type: "float", nullable: false),
                    FourthLapTime = table.Column<float>(type: "float", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trainings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TrifectaOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorseNumber1 = table.Column<short>(type: "smallint", nullable: false),
                    HorseNumber2 = table.Column<short>(type: "smallint", nullable: false),
                    HorseNumber3 = table.Column<short>(type: "smallint", nullable: false),
                    Odds = table.Column<float>(type: "float", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrifectaOdds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TrioOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorseNumber1 = table.Column<short>(type: "smallint", nullable: false),
                    HorseNumber2 = table.Column<short>(type: "smallint", nullable: false),
                    HorseNumber3 = table.Column<short>(type: "smallint", nullable: false),
                    Odds = table.Column<float>(type: "float", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrioOdds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExactaOdds");

            migrationBuilder.DropTable(
                name: "FrameNumberOdds");

            migrationBuilder.DropTable(
                name: "LearningDataCaches");

            migrationBuilder.DropTable(
                name: "QuinellaOdds");

            migrationBuilder.DropTable(
                name: "QuinellaPlaceOdds");

            migrationBuilder.DropTable(
                name: "RaceHorses");

            migrationBuilder.DropTable(
                name: "Races");

            migrationBuilder.DropTable(
                name: "Refunds");

            migrationBuilder.DropTable(
                name: "SingleOddsTimelines");

            migrationBuilder.DropTable(
                name: "SystemData");

            migrationBuilder.DropTable(
                name: "Trainings");

            migrationBuilder.DropTable(
                name: "TrifectaOdds");

            migrationBuilder.DropTable(
                name: "TrioOdds");
        }
    }
}
