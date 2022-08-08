using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddExternalNumbers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "ResultHorsesCount",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: "ExternalNumberConfigs",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    FileNamePattern = table.Column<string>(type: "TEXT", nullable: false),
                    FileFormat = table.Column<short>(type: "INTEGER", nullable: false),
                    ValuesFormat = table.Column<short>(type: "INTEGER", nullable: false),
                    DotFormat = table.Column<short>(type: "INTEGER", nullable: false),
                    SortRule = table.Column<short>(type: "INTEGER", nullable: false),
                    RaceIdFormat = table.Column<short>(type: "INTEGER", nullable: false),
                    Order = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<ushort>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalNumberConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExternalNumbers",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConfigId = table.Column<uint>(type: "INTEGER", nullable: false),
                    RaceKey = table.Column<string>(type: "TEXT", nullable: false),
                    HorseNumber = table.Column<short>(type: "INTEGER", nullable: false),
                    Value = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<ushort>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalNumbers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalNumberConfigs");

            migrationBuilder.DropTable(
                name: "ExternalNumbers");

            migrationBuilder.DropColumn(
                name: "ResultHorsesCount",
                table: "Races");
        }
    }
}
