using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddMovieStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AfterThirdHalongTimeOrder",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "UniformFormatData",
                table: "RaceHorses");

            migrationBuilder.AddColumn<short>(
                name: "MovieStatus",
                table: "WoodtipTrainings",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "MovieStatus",
                table: "Trainings",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MovieStatus",
                table: "WoodtipTrainings");

            migrationBuilder.DropColumn(
                name: "MovieStatus",
                table: "Trainings");

            migrationBuilder.AddColumn<short>(
                name: "AfterThirdHalongTimeOrder",
                table: "RaceHorses",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<byte[]>(
                name: "UniformFormatData",
                table: "RaceHorses",
                type: "VARBINARY(8000)",
                maxLength: 8000,
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
