using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KmyKeiba.Migrations
{
    public partial class AddOdds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "Age",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<float>(
                name: "PlaceOddsMax",
                table: "RaceHorses",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "PlaceOddsMin",
                table: "RaceHorses",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<short>(
                name: "Sex",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: "ExactaOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorseNumber1 = table.Column<short>(type: "smallint", nullable: false),
                    HorseNumber2 = table.Column<short>(type: "smallint", nullable: false),
                    Odds = table.Column<float>(type: "float", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExactaOdds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FrameNumberOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Frame1 = table.Column<short>(type: "smallint", nullable: false),
                    Frame2 = table.Column<short>(type: "smallint", nullable: false),
                    Odds = table.Column<float>(type: "float", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrameNumberOdds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuinellaOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorseNumber1 = table.Column<short>(type: "smallint", nullable: false),
                    HorseNumber2 = table.Column<short>(type: "smallint", nullable: false),
                    Odds = table.Column<float>(type: "float", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuinellaOdds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QuinellaPlaceOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorseNumber1 = table.Column<short>(type: "smallint", nullable: false),
                    HorseNumber2 = table.Column<short>(type: "smallint", nullable: false),
                    PlaceOddsMin = table.Column<float>(type: "float", nullable: false),
                    PlaceOddsMax = table.Column<float>(type: "float", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuinellaPlaceOdds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Refunds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SingleNumber1 = table.Column<short>(type: "smallint", nullable: false),
                    SingleNumber1Money = table.Column<int>(type: "int", nullable: false),
                    SingleNumber2 = table.Column<short>(type: "smallint", nullable: false),
                    SingleNumber2Money = table.Column<int>(type: "int", nullable: false),
                    SingleNumber3 = table.Column<short>(type: "smallint", nullable: false),
                    SingleNumber3Money = table.Column<int>(type: "int", nullable: false),
                    PlaceNumber1 = table.Column<short>(type: "smallint", nullable: false),
                    PlaceNumber1Money = table.Column<int>(type: "int", nullable: false),
                    PlaceNumber2 = table.Column<short>(type: "smallint", nullable: false),
                    PlaceNumber2Money = table.Column<int>(type: "int", nullable: false),
                    PlaceNumber3 = table.Column<short>(type: "smallint", nullable: false),
                    PlaceNumber3Money = table.Column<int>(type: "int", nullable: false),
                    PlaceNumber4 = table.Column<short>(type: "smallint", nullable: false),
                    PlaceNumber4Money = table.Column<int>(type: "int", nullable: false),
                    PlaceNumber5 = table.Column<short>(type: "smallint", nullable: false),
                    PlaceNumber5Money = table.Column<int>(type: "int", nullable: false),
                    FrameNumber1 = table.Column<short>(type: "smallint", nullable: false),
                    FrameNumber1Money = table.Column<int>(type: "int", nullable: false),
                    FrameNumber2 = table.Column<short>(type: "smallint", nullable: false),
                    FrameNumber2Money = table.Column<int>(type: "int", nullable: false),
                    FrameNumber3 = table.Column<short>(type: "smallint", nullable: false),
                    FrameNumber3Money = table.Column<int>(type: "int", nullable: false),
                    Quinella1Number1 = table.Column<short>(type: "smallint", nullable: false),
                    Quinella2Number1 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaNumber1Money = table.Column<int>(type: "int", nullable: false),
                    Quinella1Number2 = table.Column<short>(type: "smallint", nullable: false),
                    Quinella2Number2 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaNumber2Money = table.Column<int>(type: "int", nullable: false),
                    Quinella1Number3 = table.Column<short>(type: "smallint", nullable: false),
                    Quinella2Number3 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaNumber3Money = table.Column<int>(type: "int", nullable: false),
                    QuinellaPlace1Number1 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlace2Number1 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlaceNumber1Money = table.Column<int>(type: "int", nullable: false),
                    QuinellaPlace1Number2 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlace2Number2 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlaceNumber2Money = table.Column<int>(type: "int", nullable: false),
                    QuinellaPlace1Number3 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlace2Number3 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlaceNumber3Money = table.Column<int>(type: "int", nullable: false),
                    QuinellaPlace1Number4 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlace2Number4 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlaceNumber4Money = table.Column<int>(type: "int", nullable: false),
                    QuinellaPlace1Number5 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlace2Number5 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlaceNumber5Money = table.Column<int>(type: "int", nullable: false),
                    QuinellaPlace1Number6 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlace2Number6 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlaceNumber6Money = table.Column<int>(type: "int", nullable: false),
                    QuinellaPlace1Number7 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlace2Number7 = table.Column<short>(type: "smallint", nullable: false),
                    QuinellaPlaceNumber7Money = table.Column<int>(type: "int", nullable: false),
                    Exacta1Number1 = table.Column<short>(type: "smallint", nullable: false),
                    Exacta2Number1 = table.Column<short>(type: "smallint", nullable: false),
                    ExactaNumber1Money = table.Column<int>(type: "int", nullable: false),
                    Exacta1Number2 = table.Column<short>(type: "smallint", nullable: false),
                    Exacta2Number2 = table.Column<short>(type: "smallint", nullable: false),
                    ExactaNumber2Money = table.Column<int>(type: "int", nullable: false),
                    Exacta1Number3 = table.Column<short>(type: "smallint", nullable: false),
                    Exacta2Number3 = table.Column<short>(type: "smallint", nullable: false),
                    ExactaNumber3Money = table.Column<int>(type: "int", nullable: false),
                    Trio1Number1 = table.Column<short>(type: "smallint", nullable: false),
                    Trio2Number1 = table.Column<short>(type: "smallint", nullable: false),
                    Trio3Number1 = table.Column<short>(type: "smallint", nullable: false),
                    TrioNumber1Money = table.Column<int>(type: "int", nullable: false),
                    Trio1Number2 = table.Column<short>(type: "smallint", nullable: false),
                    Trio2Number2 = table.Column<short>(type: "smallint", nullable: false),
                    Trio3Number2 = table.Column<short>(type: "smallint", nullable: false),
                    TrioNumber2Money = table.Column<int>(type: "int", nullable: false),
                    Trio1Number3 = table.Column<short>(type: "smallint", nullable: false),
                    Trio2Number3 = table.Column<short>(type: "smallint", nullable: false),
                    Trio3Number3 = table.Column<short>(type: "smallint", nullable: false),
                    TrioNumber3Money = table.Column<int>(type: "int", nullable: false),
                    Trifecta1Number1 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta2Number1 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta3Number1 = table.Column<short>(type: "smallint", nullable: false),
                    TrifectaNumber1Money = table.Column<int>(type: "int", nullable: false),
                    Trifecta1Number2 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta2Number2 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta3Number2 = table.Column<short>(type: "smallint", nullable: false),
                    TrifectaNumber2Money = table.Column<int>(type: "int", nullable: false),
                    Trifecta1Number3 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta2Number3 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta3Number3 = table.Column<short>(type: "smallint", nullable: false),
                    TrifectaNumber3Money = table.Column<int>(type: "int", nullable: false),
                    Trifecta1Number4 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta2Number4 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta3Number4 = table.Column<short>(type: "smallint", nullable: false),
                    TrifectaNumber4Money = table.Column<int>(type: "int", nullable: false),
                    Trifecta1Number5 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta2Number5 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta3Number5 = table.Column<short>(type: "smallint", nullable: false),
                    TrifectaNumber5Money = table.Column<int>(type: "int", nullable: false),
                    Trifecta1Number6 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta2Number6 = table.Column<short>(type: "smallint", nullable: false),
                    Trifecta3Number6 = table.Column<short>(type: "smallint", nullable: false),
                    TrifectaNumber6Money = table.Column<int>(type: "int", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refunds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TrifectaOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorseNumber1 = table.Column<short>(type: "smallint", nullable: false),
                    HorseNumber2 = table.Column<short>(type: "smallint", nullable: false),
                    HorseNumber3 = table.Column<short>(type: "smallint", nullable: false),
                    Odds = table.Column<float>(type: "float", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrifectaOdds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TrioOdds",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaceKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorseNumber1 = table.Column<short>(type: "smallint", nullable: false),
                    HorseNumber2 = table.Column<short>(type: "smallint", nullable: false),
                    HorseNumber3 = table.Column<short>(type: "smallint", nullable: false),
                    Odds = table.Column<float>(type: "float", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataStatus = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrioOdds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExactaOdds");

            migrationBuilder.DropTable(
                name: "FrameNumberOdds");

            migrationBuilder.DropTable(
                name: "QuinellaOdds");

            migrationBuilder.DropTable(
                name: "QuinellaPlaceOdds");

            migrationBuilder.DropTable(
                name: "Refunds");

            migrationBuilder.DropTable(
                name: "TrifectaOdds");

            migrationBuilder.DropTable(
                name: "TrioOdds");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "PlaceOddsMax",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "PlaceOddsMin",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "RaceHorses");
        }
    }
}
