using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class UpdateOddsTimeline : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Odds1",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds10",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds11",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds12",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds13",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds14",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds15",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds16",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds17",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds18",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds19",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds2",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds20",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds21",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds22",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds23",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds24",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds25",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds26",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds27",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds28",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds3",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds4",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds5",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds6",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds7",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds8",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "Odds9",
                table: "SingleOddsTimelines");

            migrationBuilder.AddColumn<byte[]>(
                name: "PlaceOddsRaw",
                table: "SingleOddsTimelines",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "SingleOddsRaw",
                table: "SingleOddsTimelines",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaceOddsRaw",
                table: "SingleOddsTimelines");

            migrationBuilder.DropColumn(
                name: "SingleOddsRaw",
                table: "SingleOddsTimelines");

            migrationBuilder.AddColumn<short>(
                name: "Odds1",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds10",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds11",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds12",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds13",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds14",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds15",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds16",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds17",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds18",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds19",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds2",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds20",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds21",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds22",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds23",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds24",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds25",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds26",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds27",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds28",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds3",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds4",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds5",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds6",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds7",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds8",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Odds9",
                table: "SingleOddsTimelines",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);
        }
    }
}
