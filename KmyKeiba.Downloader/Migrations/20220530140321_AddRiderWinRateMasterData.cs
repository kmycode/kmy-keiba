using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddRiderWinRateMasterData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsContainsRiderWinRate",
                table: "RaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RiderWinRates",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RiderCode = table.Column<string>(type: "TEXT", nullable: false),
                    Year = table.Column<short>(type: "INTEGER", nullable: false),
                    Month = table.Column<short>(type: "INTEGER", nullable: false),
                    AllCount = table.Column<short>(type: "INTEGER", nullable: false),
                    FirstCount = table.Column<short>(type: "INTEGER", nullable: false),
                    SecondCount = table.Column<short>(type: "INTEGER", nullable: false),
                    ThirdCount = table.Column<short>(type: "INTEGER", nullable: false),
                    LosedCount = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<ushort>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiderWinRates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiderWinRates_RiderCode",
                table: "RiderWinRates",
                column: "RiderCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiderWinRates");

            migrationBuilder.DropColumn(
                name: "IsContainsRiderWinRate",
                table: "RaceHorses");
        }
    }
}
