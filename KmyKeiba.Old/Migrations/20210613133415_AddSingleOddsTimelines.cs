using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KmyKeiba.Migrations
{
    public partial class AddSingleOddsTimelines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SingleOddsTimelines",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Odds1 = table.Column<float>(type: "float", nullable: false),
                    Odds2 = table.Column<float>(type: "float", nullable: false),
                    Odds3 = table.Column<float>(type: "float", nullable: false),
                    Odds4 = table.Column<float>(type: "float", nullable: false),
                    Odds5 = table.Column<float>(type: "float", nullable: false),
                    Odds6 = table.Column<float>(type: "float", nullable: false),
                    Odds7 = table.Column<float>(type: "float", nullable: false),
                    Odds8 = table.Column<float>(type: "float", nullable: false),
                    Odds9 = table.Column<float>(type: "float", nullable: false),
                    Odds10 = table.Column<float>(type: "float", nullable: false),
                    Odds11 = table.Column<float>(type: "float", nullable: false),
                    Odds12 = table.Column<float>(type: "float", nullable: false),
                    Odds13 = table.Column<float>(type: "float", nullable: false),
                    Odds14 = table.Column<float>(type: "float", nullable: false),
                    Odds15 = table.Column<float>(type: "float", nullable: false),
                    Odds16 = table.Column<float>(type: "float", nullable: false),
                    Odds17 = table.Column<float>(type: "float", nullable: false),
                    Odds18 = table.Column<float>(type: "float", nullable: false),
                    Odds19 = table.Column<float>(type: "float", nullable: false),
                    Odds20 = table.Column<float>(type: "float", nullable: false),
                    Odds21 = table.Column<float>(type: "float", nullable: false),
                    Odds22 = table.Column<float>(type: "float", nullable: false),
                    Odds23 = table.Column<float>(type: "float", nullable: false),
                    Odds24 = table.Column<float>(type: "float", nullable: false),
                    Odds25 = table.Column<float>(type: "float", nullable: false),
                    Odds26 = table.Column<float>(type: "float", nullable: false),
                    Odds27 = table.Column<float>(type: "float", nullable: false),
                    Odds28 = table.Column<float>(type: "float", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleOddsTimelines", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SingleOddsTimelines");
        }
    }
}
