using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class OddsFloatToShort : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RaceKey",
                table: "TrioOdds",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<short>(
                name: "Odds",
                table: "TrioOdds",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "RaceKey",
                table: "TrifectaOdds",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<short>(
                name: "Odds",
                table: "TrifectaOdds",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "RaceKey",
                table: "SingleOddsTimelines",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<short>(
                name: "Odds9",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds8",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds7",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds6",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds5",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds4",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds3",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds28",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds27",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds26",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds25",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds24",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds23",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds22",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds21",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds20",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds2",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds19",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds18",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds17",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds16",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds15",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds14",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds13",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds12",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds11",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds10",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "Odds1",
                table: "SingleOddsTimelines",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "RaceKey",
                table: "QuinellaPlaceOdds",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<short>(
                name: "PlaceOddsMin",
                table: "QuinellaPlaceOdds",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<short>(
                name: "PlaceOddsMax",
                table: "QuinellaPlaceOdds",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "RaceKey",
                table: "QuinellaOdds",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<short>(
                name: "Odds",
                table: "QuinellaOdds",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "RaceKey",
                table: "FrameNumberOdds",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<short>(
                name: "Odds",
                table: "FrameNumberOdds",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "RaceKey",
                table: "ExactaOdds",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<short>(
                name: "Odds",
                table: "ExactaOdds",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");

            migrationBuilder.CreateIndex(
                name: "IX_TrioOdds_RaceKey",
                table: "TrioOdds",
                column: "RaceKey");

            migrationBuilder.CreateIndex(
                name: "IX_TrifectaOdds_RaceKey",
                table: "TrifectaOdds",
                column: "RaceKey");

            migrationBuilder.CreateIndex(
                name: "IX_SingleOddsTimelines_RaceKey",
                table: "SingleOddsTimelines",
                column: "RaceKey");

            migrationBuilder.CreateIndex(
                name: "IX_QuinellaPlaceOdds_RaceKey",
                table: "QuinellaPlaceOdds",
                column: "RaceKey");

            migrationBuilder.CreateIndex(
                name: "IX_QuinellaOdds_RaceKey",
                table: "QuinellaOdds",
                column: "RaceKey");

            migrationBuilder.CreateIndex(
                name: "IX_FrameNumberOdds_RaceKey",
                table: "FrameNumberOdds",
                column: "RaceKey");

            migrationBuilder.CreateIndex(
                name: "IX_ExactaOdds_RaceKey",
                table: "ExactaOdds",
                column: "RaceKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrioOdds_RaceKey",
                table: "TrioOdds");

            migrationBuilder.DropIndex(
                name: "IX_TrifectaOdds_RaceKey",
                table: "TrifectaOdds");

            migrationBuilder.DropIndex(
                name: "IX_SingleOddsTimelines_RaceKey",
                table: "SingleOddsTimelines");

            migrationBuilder.DropIndex(
                name: "IX_QuinellaPlaceOdds_RaceKey",
                table: "QuinellaPlaceOdds");

            migrationBuilder.DropIndex(
                name: "IX_QuinellaOdds_RaceKey",
                table: "QuinellaOdds");

            migrationBuilder.DropIndex(
                name: "IX_FrameNumberOdds_RaceKey",
                table: "FrameNumberOdds");

            migrationBuilder.DropIndex(
                name: "IX_ExactaOdds_RaceKey",
                table: "ExactaOdds");

            migrationBuilder.AlterColumn<string>(
                name: "RaceKey",
                table: "TrioOdds",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<float>(
                name: "Odds",
                table: "TrioOdds",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<string>(
                name: "RaceKey",
                table: "TrifectaOdds",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<float>(
                name: "Odds",
                table: "TrifectaOdds",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<string>(
                name: "RaceKey",
                table: "SingleOddsTimelines",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<float>(
                name: "Odds9",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds8",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds7",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds6",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds5",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds4",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds3",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds28",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds27",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds26",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds25",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds24",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds23",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds22",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds21",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds20",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds2",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds19",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds18",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds17",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds16",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds15",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds14",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds13",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds12",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds11",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds10",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "Odds1",
                table: "SingleOddsTimelines",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<string>(
                name: "RaceKey",
                table: "QuinellaPlaceOdds",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<float>(
                name: "PlaceOddsMin",
                table: "QuinellaPlaceOdds",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<float>(
                name: "PlaceOddsMax",
                table: "QuinellaPlaceOdds",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<string>(
                name: "RaceKey",
                table: "QuinellaOdds",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<float>(
                name: "Odds",
                table: "QuinellaOdds",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<string>(
                name: "RaceKey",
                table: "FrameNumberOdds",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<float>(
                name: "Odds",
                table: "FrameNumberOdds",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<string>(
                name: "RaceKey",
                table: "ExactaOdds",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<float>(
                name: "Odds",
                table: "ExactaOdds",
                type: "float",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");
        }
    }
}
