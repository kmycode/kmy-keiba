using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class IntToShort : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_RaceKey_Key_RiderCode_TrainerCode_RiderName_Train~",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "CourseBaseTimeCache",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "CourseBaseTimeCacheVersion",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "IsRunningStyleSetManually",
                table: "RaceHorses");

            migrationBuilder.AlterColumn<short>(
                name: "HorsesCount",
                table: "Races",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "Distance",
                table: "Races",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "CourseRaceNumber",
                table: "Races",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "ThirdCornerOrder",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "SecondCornerOrder",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "ResultOrder",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "Popular",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "Number",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "FrameNumber",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "FourthCornerOrder",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "FirstCornerOrder",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "AfterThirdHalongTimeOrder",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<short>(
                name: "CalcedRunningStyle",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_RaceKey_Key_RiderCode_TrainerCode_CourseCode",
                table: "RaceHorses",
                columns: new[] { "RaceKey", "Key", "RiderCode", "TrainerCode", "CourseCode" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RaceHorses_RaceKey_Key_RiderCode_TrainerCode_CourseCode",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "CalcedRunningStyle",
                table: "RaceHorses");

            migrationBuilder.AlterColumn<int>(
                name: "HorsesCount",
                table: "Races",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "Distance",
                table: "Races",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "CourseRaceNumber",
                table: "Races",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<float>(
                name: "CourseBaseTimeCache",
                table: "Races",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "CourseBaseTimeCacheVersion",
                table: "Races",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ThirdCornerOrder",
                table: "RaceHorses",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "SecondCornerOrder",
                table: "RaceHorses",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "ResultOrder",
                table: "RaceHorses",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "Popular",
                table: "RaceHorses",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "Number",
                table: "RaceHorses",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "FrameNumber",
                table: "RaceHorses",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "FourthCornerOrder",
                table: "RaceHorses",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "FirstCornerOrder",
                table: "RaceHorses",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "AfterThirdHalongTimeOrder",
                table: "RaceHorses",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddColumn<bool>(
                name: "IsRunningStyleSetManually",
                table: "RaceHorses",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorses_RaceKey_Key_RiderCode_TrainerCode_RiderName_Train~",
                table: "RaceHorses",
                columns: new[] { "RaceKey", "Key", "RiderCode", "TrainerCode", "RiderName", "TrainerName" });
        }
    }
}
