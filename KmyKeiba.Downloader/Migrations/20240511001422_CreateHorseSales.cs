using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    /// <inheritdoc />
    public partial class CreateHorseSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Horses_Code_MFBreedingCode",
                table: "Horses");

            migrationBuilder.CreateTable(
                name: "HorseSales",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    FatherBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    MotherBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    BornYear = table.Column<int>(type: "INTEGER", nullable: false),
                    MarketCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    MarketOwnerName = table.Column<string>(type: "TEXT", nullable: false),
                    MarketName = table.Column<string>(type: "TEXT", nullable: false),
                    MarketStartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MarketEndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Age = table.Column<short>(type: "INTEGER", nullable: false),
                    Price = table.Column<long>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorseSales", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Horses_Code",
                table: "Horses",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_HorseSales_Code",
                table: "HorseSales",
                column: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HorseSales");

            migrationBuilder.DropIndex(
                name: "IX_Horses_Code",
                table: "Horses");

            migrationBuilder.CreateIndex(
                name: "IX_Horses_Code_MFBreedingCode",
                table: "Horses",
                columns: new[] { "Code", "MFBreedingCode" });
        }
    }
}
