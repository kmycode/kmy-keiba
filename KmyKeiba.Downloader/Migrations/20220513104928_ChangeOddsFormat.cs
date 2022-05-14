using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class ChangeOddsFormat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HorseNumber1",
                table: "TrioOdds");

            migrationBuilder.DropColumn(
                name: "HorseNumber2",
                table: "TrioOdds");

            migrationBuilder.DropColumn(
                name: "HorseNumber1",
                table: "TrifectaOdds");

            migrationBuilder.DropColumn(
                name: "HorseNumber2",
                table: "TrifectaOdds");

            migrationBuilder.DropColumn(
                name: "HorseNumber1",
                table: "QuinellaPlaceOdds");

            migrationBuilder.DropColumn(
                name: "HorseNumber2",
                table: "QuinellaPlaceOdds");

            migrationBuilder.DropColumn(
                name: "PlaceOddsMax",
                table: "QuinellaPlaceOdds");

            migrationBuilder.DropColumn(
                name: "HorseNumber1",
                table: "QuinellaOdds");

            migrationBuilder.DropColumn(
                name: "Frame1",
                table: "FrameNumberOdds");

            migrationBuilder.DropColumn(
                name: "HorseNumber1",
                table: "ExactaOdds");

            migrationBuilder.RenameColumn(
                name: "HorseNumber3",
                table: "TrioOdds",
                newName: "HorsesCount");

            migrationBuilder.RenameColumn(
                name: "HorseNumber3",
                table: "TrifectaOdds",
                newName: "HorsesCount");

            migrationBuilder.RenameColumn(
                name: "PlaceOddsMin",
                table: "QuinellaPlaceOdds",
                newName: "HorsesCount");

            migrationBuilder.RenameColumn(
                name: "HorseNumber2",
                table: "QuinellaOdds",
                newName: "HorsesCount");

            migrationBuilder.RenameColumn(
                name: "Frame2",
                table: "FrameNumberOdds",
                newName: "FramesCount");

            migrationBuilder.RenameColumn(
                name: "HorseNumber2",
                table: "ExactaOdds",
                newName: "HorsesCount");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Odds",
                table: "TrioOdds",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Odds",
                table: "TrifectaOdds",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<byte[]>(
                name: "OddsMax",
                table: "QuinellaPlaceOdds",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "OddsMin",
                table: "QuinellaPlaceOdds",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<byte[]>(
                name: "Odds",
                table: "QuinellaOdds",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Odds",
                table: "FrameNumberOdds",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Odds",
                table: "ExactaOdds",
                type: "BLOB",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "INTEGER");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OddsMax",
                table: "QuinellaPlaceOdds");

            migrationBuilder.DropColumn(
                name: "OddsMin",
                table: "QuinellaPlaceOdds");

            migrationBuilder.RenameColumn(
                name: "HorsesCount",
                table: "TrioOdds",
                newName: "HorseNumber3");

            migrationBuilder.RenameColumn(
                name: "HorsesCount",
                table: "TrifectaOdds",
                newName: "HorseNumber3");

            migrationBuilder.RenameColumn(
                name: "HorsesCount",
                table: "QuinellaPlaceOdds",
                newName: "PlaceOddsMin");

            migrationBuilder.RenameColumn(
                name: "HorsesCount",
                table: "QuinellaOdds",
                newName: "HorseNumber2");

            migrationBuilder.RenameColumn(
                name: "FramesCount",
                table: "FrameNumberOdds",
                newName: "Frame2");

            migrationBuilder.RenameColumn(
                name: "HorsesCount",
                table: "ExactaOdds",
                newName: "HorseNumber2");

            migrationBuilder.AlterColumn<short>(
                name: "Odds",
                table: "TrioOdds",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AddColumn<short>(
                name: "HorseNumber1",
                table: "TrioOdds",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "HorseNumber2",
                table: "TrioOdds",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AlterColumn<short>(
                name: "Odds",
                table: "TrifectaOdds",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AddColumn<short>(
                name: "HorseNumber1",
                table: "TrifectaOdds",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "HorseNumber2",
                table: "TrifectaOdds",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "HorseNumber1",
                table: "QuinellaPlaceOdds",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "HorseNumber2",
                table: "QuinellaPlaceOdds",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "PlaceOddsMax",
                table: "QuinellaPlaceOdds",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AlterColumn<short>(
                name: "Odds",
                table: "QuinellaOdds",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AddColumn<short>(
                name: "HorseNumber1",
                table: "QuinellaOdds",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AlterColumn<short>(
                name: "Odds",
                table: "FrameNumberOdds",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AddColumn<short>(
                name: "Frame1",
                table: "FrameNumberOdds",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AlterColumn<short>(
                name: "Odds",
                table: "ExactaOdds",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.AddColumn<short>(
                name: "HorseNumber1",
                table: "ExactaOdds",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);
        }
    }
}
