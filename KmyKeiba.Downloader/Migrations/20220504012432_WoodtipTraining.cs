using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class WoodtipTraining : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WoodtipTrainings",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HorseKey = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Center = table.Column<short>(type: "smallint", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Course = table.Column<short>(type: "smallint", nullable: false),
                    Direction = table.Column<short>(type: "smallint", nullable: false),
                    Lap1Time = table.Column<float>(type: "float", nullable: false),
                    Lap2Time = table.Column<float>(type: "float", nullable: false),
                    Lap3Time = table.Column<float>(type: "float", nullable: false),
                    Lap4Time = table.Column<float>(type: "float", nullable: false),
                    Lap5Time = table.Column<float>(type: "float", nullable: false),
                    Lap6Time = table.Column<float>(type: "float", nullable: false),
                    Lap7Time = table.Column<float>(type: "float", nullable: false),
                    Lap8Time = table.Column<float>(type: "float", nullable: false),
                    Lap9Time = table.Column<float>(type: "float", nullable: false),
                    Lap10Time = table.Column<float>(type: "float", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WoodtipTrainings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_WoodtipTrainings_HorseKey",
                table: "WoodtipTrainings",
                column: "HorseKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WoodtipTrainings");
        }
    }
}
