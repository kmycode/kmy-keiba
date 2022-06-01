using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class InitializeSqlite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExactaOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaceKey = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    HorseNumber1 = table.Column<short>(type: "INTEGER", nullable: false),
                    HorseNumber2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExactaOdds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FrameNumberOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaceKey = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Frame1 = table.Column<short>(type: "INTEGER", nullable: false),
                    Frame2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrameNumberOdds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HorseBloods",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 72, nullable: false),
                    BornYear = table.Column<int>(type: "INTEGER", nullable: false),
                    Sex = table.Column<short>(type: "INTEGER", nullable: false),
                    Color = table.Column<short>(type: "INTEGER", nullable: false),
                    From = table.Column<short>(type: "INTEGER", nullable: false),
                    ProductingName = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    FatherKey = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    MotherKey = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorseBloods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Horses",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Entried = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Retired = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Born = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 72, nullable: false),
                    Sex = table.Column<short>(type: "INTEGER", nullable: false),
                    Type = table.Column<short>(type: "INTEGER", nullable: false),
                    Color = table.Column<short>(type: "INTEGER", nullable: false),
                    FatherBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    MotherBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    FFBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    FMBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    MFBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    MMBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    FFFBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    FFMBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    FMFBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    FMMBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    MFFBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    MFMBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    MMFBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    MMMBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    Belongs = table.Column<short>(type: "INTEGER", nullable: false),
                    TrainerCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    TrainerName = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    InviteFrom = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    ProducingCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    OwnerCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Horses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningDataCaches",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaceKey = table.Column<string>(type: "TEXT", nullable: false),
                    HorseName = table.Column<string>(type: "TEXT", nullable: false),
                    CacheVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    Cache = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningDataCaches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuinellaOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaceKey = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    HorseNumber1 = table.Column<short>(type: "INTEGER", nullable: false),
                    HorseNumber2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuinellaOdds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuinellaPlaceOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaceKey = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    HorseNumber1 = table.Column<short>(type: "INTEGER", nullable: false),
                    HorseNumber2 = table.Column<short>(type: "INTEGER", nullable: false),
                    PlaceOddsMin = table.Column<short>(type: "INTEGER", nullable: false),
                    PlaceOddsMax = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuinellaPlaceOdds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RaceHorses",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    RaceKey = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 72, nullable: false),
                    Age = table.Column<short>(type: "INTEGER", nullable: false),
                    Sex = table.Column<short>(type: "INTEGER", nullable: false),
                    Type = table.Column<short>(type: "INTEGER", nullable: false),
                    Color = table.Column<short>(type: "INTEGER", nullable: false),
                    CourseCode = table.Column<short>(type: "INTEGER", nullable: false),
                    Mark = table.Column<short>(type: "INTEGER", nullable: false),
                    Number = table.Column<short>(type: "INTEGER", nullable: false),
                    FrameNumber = table.Column<short>(type: "INTEGER", nullable: false),
                    ResultOrder = table.Column<short>(type: "INTEGER", nullable: false),
                    ResultLength1 = table.Column<short>(type: "INTEGER", nullable: false),
                    ResultLength2 = table.Column<short>(type: "INTEGER", nullable: false),
                    ResultLength3 = table.Column<short>(type: "INTEGER", nullable: false),
                    AbnormalResult = table.Column<short>(type: "INTEGER", nullable: false),
                    Popular = table.Column<short>(type: "INTEGER", nullable: false),
                    ResultTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    FirstCornerOrder = table.Column<short>(type: "INTEGER", nullable: false),
                    SecondCornerOrder = table.Column<short>(type: "INTEGER", nullable: false),
                    ThirdCornerOrder = table.Column<short>(type: "INTEGER", nullable: false),
                    FourthCornerOrder = table.Column<short>(type: "INTEGER", nullable: false),
                    RiderCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    RiderName = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    RiderWeight = table.Column<short>(type: "INTEGER", nullable: false),
                    TrainerCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    TrainerName = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    OwnerCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    OwnerName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    IsBlinkers = table.Column<bool>(type: "INTEGER", nullable: false),
                    Weight = table.Column<short>(type: "INTEGER", nullable: false),
                    WeightDiff = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds = table.Column<short>(type: "INTEGER", nullable: false),
                    PlaceOddsMin = table.Column<short>(type: "INTEGER", nullable: false),
                    PlaceOddsMax = table.Column<short>(type: "INTEGER", nullable: false),
                    AfterThirdHalongTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    AfterThirdHalongTimeOrder = table.Column<short>(type: "INTEGER", nullable: false),
                    RunningStyle = table.Column<short>(type: "INTEGER", nullable: false),
                    IsRunningStyleSetManually = table.Column<bool>(type: "INTEGER", nullable: false),
                    PreviousRaceDays = table.Column<short>(type: "INTEGER", nullable: false),
                    UniformFormat = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    UniformFormatData = table.Column<byte[]>(type: "VARBINARY(8000)", maxLength: 8000, nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceHorses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Races",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Name6Chars = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                    SubName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Course = table.Column<short>(type: "INTEGER", nullable: false),
                    CourseType = table.Column<string>(type: "TEXT", maxLength: 4, nullable: false),
                    TrackGround = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackCornerDirection = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackType = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackOption = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackWeather = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackCondition = table.Column<short>(type: "INTEGER", nullable: false),
                    Distance = table.Column<short>(type: "INTEGER", nullable: false),
                    CourseRaceNumber = table.Column<short>(type: "INTEGER", nullable: false),
                    SubjectName = table.Column<string>(type: "TEXT", nullable: false),
                    Grade = table.Column<int>(type: "INTEGER", nullable: false),
                    SubjectAge2 = table.Column<int>(type: "INTEGER", nullable: false),
                    SubjectAge3 = table.Column<int>(type: "INTEGER", nullable: false),
                    SubjectAge4 = table.Column<int>(type: "INTEGER", nullable: false),
                    SubjectAge5 = table.Column<int>(type: "INTEGER", nullable: false),
                    SubjectAgeYounger = table.Column<int>(type: "INTEGER", nullable: false),
                    HorsesCount = table.Column<short>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Corner1Result = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Corner1Position = table.Column<short>(type: "INTEGER", nullable: false),
                    Corner1Number = table.Column<short>(type: "INTEGER", nullable: false),
                    Corner1LapTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Corner2Result = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Corner2Position = table.Column<short>(type: "INTEGER", nullable: false),
                    Corner2Number = table.Column<short>(type: "INTEGER", nullable: false),
                    Corner2LapTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Corner3Result = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Corner3Position = table.Column<short>(type: "INTEGER", nullable: false),
                    Corner3Number = table.Column<short>(type: "INTEGER", nullable: false),
                    Corner3LapTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Corner4Result = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Corner4Position = table.Column<short>(type: "INTEGER", nullable: false),
                    Corner4Number = table.Column<short>(type: "INTEGER", nullable: false),
                    Corner4LapTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Races", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RaceStandardTimes",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Course = table.Column<short>(type: "INTEGER", nullable: false),
                    SampleStartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SampleEndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SampleCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CornerDirection = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackOption = table.Column<short>(type: "INTEGER", nullable: false),
                    Ground = table.Column<short>(type: "INTEGER", nullable: false),
                    Weather = table.Column<short>(type: "INTEGER", nullable: false),
                    Condition = table.Column<short>(type: "INTEGER", nullable: false),
                    TrackType = table.Column<short>(type: "INTEGER", nullable: false),
                    Distance = table.Column<short>(type: "INTEGER", nullable: false),
                    DistanceMax = table.Column<short>(type: "INTEGER", nullable: false),
                    Average = table.Column<double>(type: "REAL", nullable: false),
                    Median = table.Column<double>(type: "REAL", nullable: false),
                    Deviation = table.Column<double>(type: "REAL", nullable: false),
                    A3FAverage = table.Column<double>(type: "REAL", nullable: false),
                    A3FMedian = table.Column<double>(type: "REAL", nullable: false),
                    A3FDeviation = table.Column<double>(type: "REAL", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<ushort>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceStandardTimes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Refunds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaceKey = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SingleNumber1 = table.Column<short>(type: "INTEGER", nullable: false),
                    SingleNumber1Money = table.Column<int>(type: "INTEGER", nullable: false),
                    SingleNumber2 = table.Column<short>(type: "INTEGER", nullable: false),
                    SingleNumber2Money = table.Column<int>(type: "INTEGER", nullable: false),
                    SingleNumber3 = table.Column<short>(type: "INTEGER", nullable: false),
                    SingleNumber3Money = table.Column<int>(type: "INTEGER", nullable: false),
                    PlaceNumber1 = table.Column<short>(type: "INTEGER", nullable: false),
                    PlaceNumber1Money = table.Column<int>(type: "INTEGER", nullable: false),
                    PlaceNumber2 = table.Column<short>(type: "INTEGER", nullable: false),
                    PlaceNumber2Money = table.Column<int>(type: "INTEGER", nullable: false),
                    PlaceNumber3 = table.Column<short>(type: "INTEGER", nullable: false),
                    PlaceNumber3Money = table.Column<int>(type: "INTEGER", nullable: false),
                    PlaceNumber4 = table.Column<short>(type: "INTEGER", nullable: false),
                    PlaceNumber4Money = table.Column<int>(type: "INTEGER", nullable: false),
                    PlaceNumber5 = table.Column<short>(type: "INTEGER", nullable: false),
                    PlaceNumber5Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Frame1Number1 = table.Column<short>(type: "INTEGER", nullable: false),
                    Frame2Number1 = table.Column<short>(type: "INTEGER", nullable: false),
                    FrameNumber1Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Frame1Number2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Frame2Number2 = table.Column<short>(type: "INTEGER", nullable: false),
                    FrameNumber2Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Frame1Number3 = table.Column<short>(type: "INTEGER", nullable: false),
                    Frame2Number3 = table.Column<short>(type: "INTEGER", nullable: false),
                    FrameNumber3Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Quinella1Number1 = table.Column<short>(type: "INTEGER", nullable: false),
                    Quinella2Number1 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaNumber1Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Quinella1Number2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Quinella2Number2 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaNumber2Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Quinella1Number3 = table.Column<short>(type: "INTEGER", nullable: false),
                    Quinella2Number3 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaNumber3Money = table.Column<int>(type: "INTEGER", nullable: false),
                    QuinellaPlace1Number1 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaPlace2Number1 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaPlaceNumber1Money = table.Column<int>(type: "INTEGER", nullable: false),
                    QuinellaPlace1Number2 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaPlace2Number2 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaPlaceNumber2Money = table.Column<int>(type: "INTEGER", nullable: false),
                    QuinellaPlace1Number3 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaPlace2Number3 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaPlaceNumber3Money = table.Column<int>(type: "INTEGER", nullable: false),
                    QuinellaPlace1Number4 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaPlace2Number4 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaPlaceNumber4Money = table.Column<int>(type: "INTEGER", nullable: false),
                    QuinellaPlace1Number5 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaPlace2Number5 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaPlaceNumber5Money = table.Column<int>(type: "INTEGER", nullable: false),
                    QuinellaPlace1Number6 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaPlace2Number6 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaPlaceNumber6Money = table.Column<int>(type: "INTEGER", nullable: false),
                    QuinellaPlace1Number7 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaPlace2Number7 = table.Column<short>(type: "INTEGER", nullable: false),
                    QuinellaPlaceNumber7Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Exacta1Number1 = table.Column<short>(type: "INTEGER", nullable: false),
                    Exacta2Number1 = table.Column<short>(type: "INTEGER", nullable: false),
                    ExactaNumber1Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Exacta1Number2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Exacta2Number2 = table.Column<short>(type: "INTEGER", nullable: false),
                    ExactaNumber2Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Exacta1Number3 = table.Column<short>(type: "INTEGER", nullable: false),
                    Exacta2Number3 = table.Column<short>(type: "INTEGER", nullable: false),
                    ExactaNumber3Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Trio1Number1 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trio2Number1 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trio3Number1 = table.Column<short>(type: "INTEGER", nullable: false),
                    TrioNumber1Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Trio1Number2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trio2Number2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trio3Number2 = table.Column<short>(type: "INTEGER", nullable: false),
                    TrioNumber2Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Trio1Number3 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trio2Number3 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trio3Number3 = table.Column<short>(type: "INTEGER", nullable: false),
                    TrioNumber3Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Trifecta1Number1 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trifecta2Number1 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trifecta3Number1 = table.Column<short>(type: "INTEGER", nullable: false),
                    TrifectaNumber1Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Trifecta1Number2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trifecta2Number2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trifecta3Number2 = table.Column<short>(type: "INTEGER", nullable: false),
                    TrifectaNumber2Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Trifecta1Number3 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trifecta2Number3 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trifecta3Number3 = table.Column<short>(type: "INTEGER", nullable: false),
                    TrifectaNumber3Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Trifecta1Number4 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trifecta2Number4 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trifecta3Number4 = table.Column<short>(type: "INTEGER", nullable: false),
                    TrifectaNumber4Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Trifecta1Number5 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trifecta2Number5 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trifecta3Number5 = table.Column<short>(type: "INTEGER", nullable: false),
                    TrifectaNumber5Money = table.Column<int>(type: "INTEGER", nullable: false),
                    Trifecta1Number6 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trifecta2Number6 = table.Column<short>(type: "INTEGER", nullable: false),
                    Trifecta3Number6 = table.Column<short>(type: "INTEGER", nullable: false),
                    TrifectaNumber6Money = table.Column<int>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refunds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SingleOddsTimelines",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaceKey = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Odds1 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds3 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds4 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds5 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds6 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds7 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds8 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds9 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds10 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds11 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds12 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds13 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds14 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds15 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds16 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds17 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds18 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds19 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds20 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds21 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds22 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds23 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds24 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds25 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds26 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds27 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds28 = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleOddsTimelines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemData",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trainings",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HorseKey = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Center = table.Column<short>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FirstLapTime = table.Column<short>(type: "INTEGER", nullable: false),
                    SecondLapTime = table.Column<short>(type: "INTEGER", nullable: false),
                    ThirdLapTime = table.Column<short>(type: "INTEGER", nullable: false),
                    FourthLapTime = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trainings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrifectaOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaceKey = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    HorseNumber1 = table.Column<short>(type: "INTEGER", nullable: false),
                    HorseNumber2 = table.Column<short>(type: "INTEGER", nullable: false),
                    HorseNumber3 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrifectaOdds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrioOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaceKey = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    HorseNumber1 = table.Column<short>(type: "INTEGER", nullable: false),
                    HorseNumber2 = table.Column<short>(type: "INTEGER", nullable: false),
                    HorseNumber3 = table.Column<short>(type: "INTEGER", nullable: false),
                    Odds = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrioOdds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WoodtipTrainings",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HorseKey = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Center = table.Column<short>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Course = table.Column<short>(type: "INTEGER", nullable: false),
                    Direction = table.Column<short>(type: "INTEGER", nullable: false),
                    Lap1Time = table.Column<short>(type: "INTEGER", nullable: false),
                    Lap2Time = table.Column<short>(type: "INTEGER", nullable: false),
                    Lap3Time = table.Column<short>(type: "INTEGER", nullable: false),
                    Lap4Time = table.Column<short>(type: "INTEGER", nullable: false),
                    Lap5Time = table.Column<short>(type: "INTEGER", nullable: false),
                    Lap6Time = table.Column<short>(type: "INTEGER", nullable: false),
                    Lap7Time = table.Column<short>(type: "INTEGER", nullable: false),
                    Lap8Time = table.Column<short>(type: "INTEGER", nullable: false),
                    Lap9Time = table.Column<short>(type: "INTEGER", nullable: false),
                    Lap10Time = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WoodtipTrainings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExactaOdds_RaceKey",
                table: "ExactaOdds",
                column: "RaceKey");

            migrationBuilder.CreateIndex(
                name: "IX_FrameNumberOdds_RaceKey",
                table: "FrameNumberOdds",
                column: "RaceKey");

            migrationBuilder.CreateIndex(
                name: "IX_HorseBloods_Key_Code",
                table: "HorseBloods",
                columns: new[] { "Key", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_Horses_Code_FatherBreedingCode_MotherBreedingCode",
                table: "Horses",
                columns: new[] { "Code", "FatherBreedingCode", "MotherBreedingCode" });

            migrationBuilder.CreateIndex(
                name: "IX_QuinellaOdds_RaceKey",
                table: "QuinellaOdds",
                column: "RaceKey");

            migrationBuilder.CreateIndex(
                name: "IX_QuinellaPlaceOdds_RaceKey",
                table: "QuinellaPlaceOdds",
                column: "RaceKey");

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_RaceKey_Key_RiderCode_TrainerCode_CourseCode",
                table: "RaceHorses",
                columns: new[] { "RaceKey", "Key", "RiderCode", "TrainerCode", "CourseCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Races_Key_Name_Course",
                table: "Races",
                columns: new[] { "Key", "Name", "Course" });

            migrationBuilder.CreateIndex(
                name: "IX_RaceStandardTimes_Course",
                table: "RaceStandardTimes",
                column: "Course");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_RaceKey",
                table: "Refunds",
                column: "RaceKey");

            migrationBuilder.CreateIndex(
                name: "IX_SingleOddsTimelines_RaceKey",
                table: "SingleOddsTimelines",
                column: "RaceKey");

            migrationBuilder.CreateIndex(
                name: "IX_Trainings_HorseKey",
                table: "Trainings",
                column: "HorseKey");

            migrationBuilder.CreateIndex(
                name: "IX_TrifectaOdds_RaceKey",
                table: "TrifectaOdds",
                column: "RaceKey");

            migrationBuilder.CreateIndex(
                name: "IX_TrioOdds_RaceKey",
                table: "TrioOdds",
                column: "RaceKey");

            migrationBuilder.CreateIndex(
                name: "IX_WoodtipTrainings_HorseKey",
                table: "WoodtipTrainings",
                column: "HorseKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExactaOdds");

            migrationBuilder.DropTable(
                name: "FrameNumberOdds");

            migrationBuilder.DropTable(
                name: "HorseBloods");

            migrationBuilder.DropTable(
                name: "Horses");

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
                name: "RaceStandardTimes");

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

            migrationBuilder.DropTable(
                name: "WoodtipTrainings");
        }
    }
}
