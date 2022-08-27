using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddRaceHaronTimes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "AfterHaronTime3",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "AfterHaronTime4",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BeforeHaronTime3",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BeforeHaronTime4",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AfterHaronTime3",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "AfterHaronTime4",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "BeforeHaronTime3",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "BeforeHaronTime4",
                table: "Races");
        }
    }
}
