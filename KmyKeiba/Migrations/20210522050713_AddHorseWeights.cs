using Microsoft.EntityFrameworkCore.Migrations;

namespace KmyKeiba.Migrations
{
    public partial class AddHorseWeights : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "TrackType",
                table: "Races",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "TrackOption",
                table: "Races",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "TrackGround",
                table: "Races",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "TrackCornerDirection",
                table: "Races",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<short>(
                name: "Course",
                table: "Races",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "DataStatus",
                table: "Races",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<float>(
                name: "Odds",
                table: "RaceHorses",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AddColumn<int>(
                name: "DataStatus",
                table: "RaceHorses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlinkers",
                table: "RaceHorses",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "RiderWeight",
                table: "RaceHorses",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<short>(
                name: "RunningStyle",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Weight",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "WeightDiff",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataStatus",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "DataStatus",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "IsBlinkers",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "RiderWeight",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "RunningStyle",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "WeightDiff",
                table: "RaceHorses");

            migrationBuilder.AlterColumn<int>(
                name: "TrackType",
                table: "Races",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "TrackOption",
                table: "Races",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "TrackGround",
                table: "Races",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "TrackCornerDirection",
                table: "Races",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "Course",
                table: "Races",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<double>(
                name: "Odds",
                table: "RaceHorses",
                type: "double",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");
        }
    }
}
