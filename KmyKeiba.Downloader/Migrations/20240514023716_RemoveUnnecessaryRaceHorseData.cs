using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnnecessaryRaceHorseData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsContainsRiderWinRate",
                table: "RaceHorses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsContainsRiderWinRate",
                table: "RaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
