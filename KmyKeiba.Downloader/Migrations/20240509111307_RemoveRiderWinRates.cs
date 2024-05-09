using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRiderWinRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiderWinRates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RiderWinRates",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AllDirtCount = table.Column<short>(type: "INTEGER", nullable: false),
                    AllDirtSteepsCount = table.Column<short>(type: "INTEGER", nullable: false),
                    AllTurfCount = table.Column<short>(type: "INTEGER", nullable: false),
                    AllTurfSteepsCount = table.Column<short>(type: "INTEGER", nullable: false),
                    Distance = table.Column<short>(type: "INTEGER", nullable: false),
                    DistanceMax = table.Column<short>(type: "INTEGER", nullable: false),
                    FirstDirtCount = table.Column<short>(type: "INTEGER", nullable: false),
                    FirstDirtSteepsCount = table.Column<short>(type: "INTEGER", nullable: false),
                    FirstTurfCount = table.Column<short>(type: "INTEGER", nullable: false),
                    FirstTurfSteepsCount = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Month = table.Column<short>(type: "INTEGER", nullable: false),
                    RiderCode = table.Column<string>(type: "TEXT", nullable: false),
                    SecondDirtCount = table.Column<short>(type: "INTEGER", nullable: false),
                    SecondDirtSteepsCount = table.Column<short>(type: "INTEGER", nullable: false),
                    SecondTurfCount = table.Column<short>(type: "INTEGER", nullable: false),
                    SecondTurfSteepsCount = table.Column<short>(type: "INTEGER", nullable: false),
                    ThirdDirtCount = table.Column<short>(type: "INTEGER", nullable: false),
                    ThirdDirtSteepsCount = table.Column<short>(type: "INTEGER", nullable: false),
                    ThirdTurfCount = table.Column<short>(type: "INTEGER", nullable: false),
                    ThirdTurfSteepsCount = table.Column<short>(type: "INTEGER", nullable: false),
                    Version = table.Column<ushort>(type: "INTEGER", nullable: false),
                    Year = table.Column<short>(type: "INTEGER", nullable: false)
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
    }
}
