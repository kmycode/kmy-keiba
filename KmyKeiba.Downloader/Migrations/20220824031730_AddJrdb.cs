using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddJrdb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JrdbRaceHorses",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaceKey = table.Column<string>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    RunningStyle = table.Column<short>(type: "INTEGER", nullable: false),
                    DistanceAptitude = table.Column<short>(type: "INTEGER", nullable: false),
                    Climb = table.Column<short>(type: "INTEGER", nullable: false),
                    IdmPoint = table.Column<short>(type: "INTEGER", nullable: false),
                    RiderPoint = table.Column<short>(type: "INTEGER", nullable: false),
                    InfoPoint = table.Column<short>(type: "INTEGER", nullable: false),
                    TotalPoint = table.Column<short>(type: "INTEGER", nullable: false),
                    BaseOdds = table.Column<short>(type: "INTEGER", nullable: false),
                    BasePopular = table.Column<short>(type: "INTEGER", nullable: false),
                    BasePlaceBetsOdds = table.Column<short>(type: "INTEGER", nullable: false),
                    BasePlaceBetsPopular = table.Column<short>(type: "INTEGER", nullable: false),
                    IdentificationMarkCount1 = table.Column<short>(type: "INTEGER", nullable: false),
                    IdentificationMarkCount2 = table.Column<short>(type: "INTEGER", nullable: false),
                    IdentificationMarkCount3 = table.Column<short>(type: "INTEGER", nullable: false),
                    IdentificationMarkCount4 = table.Column<short>(type: "INTEGER", nullable: false),
                    IdentificationMarkCount5 = table.Column<short>(type: "INTEGER", nullable: false),
                    TotalMarkCount1 = table.Column<short>(type: "INTEGER", nullable: false),
                    TotalMarkCount2 = table.Column<short>(type: "INTEGER", nullable: false),
                    TotalMarkCount3 = table.Column<short>(type: "INTEGER", nullable: false),
                    TotalMarkCount4 = table.Column<short>(type: "INTEGER", nullable: false),
                    TotalMarkCount5 = table.Column<short>(type: "INTEGER", nullable: false),
                    PopularPoint = table.Column<short>(type: "INTEGER", nullable: false),
                    TrainingPoint = table.Column<short>(type: "INTEGER", nullable: false),
                    StablePoint = table.Column<short>(type: "INTEGER", nullable: false),
                    TrainingArrow = table.Column<short>(type: "INTEGER", nullable: false),
                    StableEvaluation = table.Column<short>(type: "INTEGER", nullable: false),
                    RiderTopRatioExpectPoint = table.Column<short>(type: "INTEGER", nullable: false),
                    SpeedPoint = table.Column<short>(type: "INTEGER", nullable: false),
                    Hoof = table.Column<short>(type: "INTEGER", nullable: false),
                    YieldingAptitude = table.Column<short>(type: "INTEGER", nullable: false),
                    ClassAptitude = table.Column<short>(type: "INTEGER", nullable: false),
                    TotalMark = table.Column<short>(type: "INTEGER", nullable: false),
                    IdmMark = table.Column<short>(type: "INTEGER", nullable: false),
                    InfoMark = table.Column<short>(type: "INTEGER", nullable: false),
                    RiderMark = table.Column<short>(type: "INTEGER", nullable: false),
                    StableMark = table.Column<short>(type: "INTEGER", nullable: false),
                    TrainingMark = table.Column<short>(type: "INTEGER", nullable: false),
                    SpeedMark = table.Column<short>(type: "INTEGER", nullable: false),
                    TurfMark = table.Column<short>(type: "INTEGER", nullable: false),
                    DirtMark = table.Column<short>(type: "INTEGER", nullable: false),
                    RaceBefore3Point = table.Column<short>(type: "INTEGER", nullable: false),
                    RaceBasePoint = table.Column<short>(type: "INTEGER", nullable: false),
                    RaceAfter3Point = table.Column<short>(type: "INTEGER", nullable: false),
                    RacePositionPoint = table.Column<short>(type: "INTEGER", nullable: false),
                    RacePace = table.Column<short>(type: "INTEGER", nullable: false),
                    RaceDevelopment = table.Column<short>(type: "INTEGER", nullable: false),
                    MiddleOrder = table.Column<short>(type: "INTEGER", nullable: false),
                    MiddleDiff = table.Column<short>(type: "INTEGER", nullable: false),
                    MiddlePosition = table.Column<short>(type: "INTEGER", nullable: false),
                    After3Order = table.Column<short>(type: "INTEGER", nullable: false),
                    After3Diff = table.Column<short>(type: "INTEGER", nullable: false),
                    After3Position = table.Column<short>(type: "INTEGER", nullable: false),
                    GoalOrder = table.Column<short>(type: "INTEGER", nullable: false),
                    GoalDiff = table.Column<short>(type: "INTEGER", nullable: false),
                    GoalPosition = table.Column<short>(type: "INTEGER", nullable: false),
                    BeforeWeight = table.Column<short>(type: "INTEGER", nullable: false),
                    BeforeWeightDiff = table.Column<short>(type: "INTEGER", nullable: false),
                    SpeedPointOrder = table.Column<short>(type: "INTEGER", nullable: false),
                    LsPointOrder = table.Column<short>(type: "INTEGER", nullable: false),
                    RaceBefore3PointOrder = table.Column<short>(type: "INTEGER", nullable: false),
                    RaceBasePointOrder = table.Column<short>(type: "INTEGER", nullable: false),
                    RaceAfter3PointOrder = table.Column<short>(type: "INTEGER", nullable: false),
                    RacePositionPointOrder = table.Column<short>(type: "INTEGER", nullable: false),
                    RiderExpectOdds = table.Column<short>(type: "INTEGER", nullable: false),
                    RiderExpectPlaceBetsOdds = table.Column<short>(type: "INTEGER", nullable: false),
                    Shipping = table.Column<short>(type: "INTEGER", nullable: false),
                    Note1 = table.Column<short>(type: "INTEGER", nullable: false),
                    Note2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Note3 = table.Column<short>(type: "INTEGER", nullable: false),
                    RaceStartPoint = table.Column<short>(type: "INTEGER", nullable: false),
                    RaceStartDelayPoint = table.Column<short>(type: "INTEGER", nullable: false),
                    BigTicketPoint = table.Column<short>(type: "INTEGER", nullable: false),
                    BigTicketMark = table.Column<short>(type: "INTEGER", nullable: false),
                    LossClassSize = table.Column<short>(type: "INTEGER", nullable: false),
                    SpeedType = table.Column<short>(type: "INTEGER", nullable: false),
                    RestReason = table.Column<short>(type: "INTEGER", nullable: false),
                    GroundFlag = table.Column<short>(type: "INTEGER", nullable: false),
                    DistanceFlag = table.Column<short>(type: "INTEGER", nullable: false),
                    ClassFlag = table.Column<short>(type: "INTEGER", nullable: false),
                    ChangeStableFlag = table.Column<short>(type: "INTEGER", nullable: false),
                    CastrationFlag = table.Column<short>(type: "INTEGER", nullable: false),
                    ChangeRiderFlag = table.Column<short>(type: "INTEGER", nullable: false),
                    GrazingName = table.Column<string>(type: "TEXT", nullable: false),
                    GrazingRank = table.Column<short>(type: "INTEGER", nullable: false),
                    StableRank = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<ushort>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JrdbRaceHorses", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JrdbRaceHorses");
        }
    }
}
