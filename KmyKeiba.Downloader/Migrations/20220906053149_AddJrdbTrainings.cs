using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddJrdbTrainings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "CatchupAbreastAge",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CatchupAbreastClass",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CatchupAbreastResult",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CatchupAbreastRunningType",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CatchupAfter3hTime",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CatchupAfter3hTimePoint",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CatchupBaseTime",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CatchupBaseTimePoint",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CatchupBefore3hTime",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CatchupBefore3hTimePoint",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CatchupFollowingStatus",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CatchupHarons",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CatchupPoint",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CatchupRiderType",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CatchupRunningType",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "CatchupTrainingCourse",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CatchupTrainingDate",
                table: "RaceHorseExtras",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<short>(
                name: "CatchupTrainingWeekday",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrainingDirt",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrainingPolyTruck",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrainingPool",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrainingSlop",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrainingSteeplechase",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrainingTurf",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrainingWood",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<short>(
                name: "LastWeekTrainingCourseType",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "LastWeekTrainingTimePoint",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "TrainingCatchupPoint",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "TrainingCatchupPointDiffType",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "TrainingComment",
                table: "RaceHorseExtras",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TrainingCommentDate",
                table: "RaceHorseExtras",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<short>(
                name: "TrainingCourseType",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "TrainingDistance",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "TrainingEmphasis",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "TrainingEvaluation",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "TrainingFinishPoint",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "TrainingSizeEvaluation",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "TrainingType",
                table: "RaceHorseExtras",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CatchupAbreastAge",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupAbreastClass",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupAbreastResult",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupAbreastRunningType",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupAfter3hTime",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupAfter3hTimePoint",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupBaseTime",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupBaseTimePoint",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupBefore3hTime",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupBefore3hTimePoint",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupFollowingStatus",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupHarons",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupPoint",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupRiderType",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupRunningType",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupTrainingCourse",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupTrainingDate",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "CatchupTrainingWeekday",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "IsTrainingDirt",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "IsTrainingPolyTruck",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "IsTrainingPool",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "IsTrainingSlop",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "IsTrainingSteeplechase",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "IsTrainingTurf",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "IsTrainingWood",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "LastWeekTrainingCourseType",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "LastWeekTrainingTimePoint",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "TrainingCatchupPoint",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "TrainingCatchupPointDiffType",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "TrainingComment",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "TrainingCommentDate",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "TrainingCourseType",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "TrainingDistance",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "TrainingEmphasis",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "TrainingEvaluation",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "TrainingFinishPoint",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "TrainingSizeEvaluation",
                table: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "TrainingType",
                table: "RaceHorseExtras");
        }
    }
}
