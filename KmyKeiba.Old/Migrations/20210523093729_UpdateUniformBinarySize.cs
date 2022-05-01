using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KmyKeiba.Migrations
{
    public partial class UpdateUniformBinarySize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "UniformFormatData",
                table: "RaceHorses",
                type: "VARBINARY(8000)",
                maxLength: 8000,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "VARBINARY(7500)",
                oldMaxLength: 7500);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "UniformFormatData",
                table: "RaceHorses",
                type: "VARBINARY(7500)",
                maxLength: 7500,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "VARBINARY(8000)",
                oldMaxLength: 8000);
        }
    }
}
