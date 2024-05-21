using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    /// <inheritdoc />
    public partial class CreatePlaceOdds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlaceOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaceKey = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PlaceOddsRaw = table.Column<byte[]>(type: "BLOB", nullable: false),
                    IsCopied = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaceOdds", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaceOdds_IsCopied",
                table: "PlaceOdds",
                column: "IsCopied");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceOdds_RaceKey",
                table: "PlaceOdds",
                column: "RaceKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaceOdds");
        }
    }
}
