using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class ResultLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "ResultLength1",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "ResultLength2",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "ResultLength3",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResultLength1",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "ResultLength2",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "ResultLength3",
                table: "RaceHorses");
        }
    }
}
