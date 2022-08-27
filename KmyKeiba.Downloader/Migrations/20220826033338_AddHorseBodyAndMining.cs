using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddHorseBodyAndMining : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "BodyAss",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyBack",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyBackLength",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyBackTsunagiLength",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyBackWalkLength",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyBellyBag",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyChest",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyFrontLength",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyFrontTsunagiLength",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyFrontWalkLength",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyHead",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyLength",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyNeck",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyNote1",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyNote2",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyNote3",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyShoulder",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyStyle",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyTail",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyTailAction",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "BodyTomo",
                table: "JrdbRaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: "RaceHorseExtras",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaceKey = table.Column<string>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    MiningTime = table.Column<short>(type: "INTEGER", nullable: false),
                    MiningTimeDiffLonger = table.Column<short>(type: "INTEGER", nullable: false),
                    MiningTimeDiffShorter = table.Column<short>(type: "INTEGER", nullable: false),
                    MiningMatchScore = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<ushort>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceHorseExtras", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RaceHorseExtras_RaceKey_Key",
                table: "RaceHorseExtras",
                columns: new[] { "RaceKey", "Key" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RaceHorseExtras");

            migrationBuilder.DropColumn(
                name: "BodyAss",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyBack",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyBackLength",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyBackTsunagiLength",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyBackWalkLength",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyBellyBag",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyChest",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyFrontLength",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyFrontTsunagiLength",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyFrontWalkLength",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyHead",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyLength",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyNeck",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyNote1",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyNote2",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyNote3",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyShoulder",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyStyle",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyTail",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyTailAction",
                table: "JrdbRaceHorses");

            migrationBuilder.DropColumn(
                name: "BodyTomo",
                table: "JrdbRaceHorses");
        }
    }
}
