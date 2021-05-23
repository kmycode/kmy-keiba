using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KmyKeiba.Migrations
{
    public partial class AddHorseUniform : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "CourseCode",
                table: "RaceHorses",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "UniformFormat",
                table: "RaceHorses",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<byte[]>(
                name: "UniformFormatData",
                table: "RaceHorses",
                type: "VARBINARY(7500)",
                maxLength: 7500,
                nullable: false,
                defaultValue: new byte[0]);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseCode",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "UniformFormat",
                table: "RaceHorses");

            migrationBuilder.DropColumn(
                name: "UniformFormatData",
                table: "RaceHorses");
        }
    }
}
