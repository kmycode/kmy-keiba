using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddHorseBloodFamily : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HorseBloodInfos",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    FamilyId = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    FamilyName = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorseBloodInfos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HorseBloodInfos_Key",
                table: "HorseBloodInfos",
                column: "Key");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HorseBloodInfos");
        }
    }
}
