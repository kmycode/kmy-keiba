using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class FloatToShort : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "Lap9Time",
                table: "WoodtipTrainings",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Lap8Time",
                table: "WoodtipTrainings",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Lap7Time",
                table: "WoodtipTrainings",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Lap6Time",
                table: "WoodtipTrainings",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Lap5Time",
                table: "WoodtipTrainings",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Lap4Time",
                table: "WoodtipTrainings",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Lap3Time",
                table: "WoodtipTrainings",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Lap2Time",
                table: "WoodtipTrainings",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Lap1Time",
                table: "WoodtipTrainings",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Lap10Time",
                table: "WoodtipTrainings",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "ThirdLapTime",
                table: "Trainings",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "SecondLapTime",
                table: "Trainings",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "FourthLapTime",
                table: "Trainings",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "FirstLapTime",
                table: "Trainings",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "RiderWeight",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "PlaceOddsMin",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "PlaceOddsMax",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Lap9Time",
                table: "WoodtipTrainings",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Lap8Time",
                table: "WoodtipTrainings",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Lap7Time",
                table: "WoodtipTrainings",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Lap6Time",
                table: "WoodtipTrainings",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Lap5Time",
                table: "WoodtipTrainings",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Lap4Time",
                table: "WoodtipTrainings",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Lap3Time",
                table: "WoodtipTrainings",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Lap2Time",
                table: "WoodtipTrainings",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Lap1Time",
                table: "WoodtipTrainings",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Lap10Time",
                table: "WoodtipTrainings",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "ThirdLapTime",
                table: "Trainings",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "SecondLapTime",
                table: "Trainings",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "FourthLapTime",
                table: "Trainings",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "FirstLapTime",
                table: "Trainings",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "RiderWeight",
                table: "RaceHorses",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "PlaceOddsMin",
                table: "RaceHorses",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "PlaceOddsMax",
                table: "RaceHorses",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds",
                table: "RaceHorses",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");
        }
    }
}
