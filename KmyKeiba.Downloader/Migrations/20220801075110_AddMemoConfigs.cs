using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddMemoConfigs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemoConfigs",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Header = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<short>(type: "INTEGER", nullable: false),
                    Order = table.Column<short>(type: "INTEGER", nullable: false),
                    Style = table.Column<short>(type: "INTEGER", nullable: false),
                    MemoNumber = table.Column<short>(type: "INTEGER", nullable: false),
                    Target1 = table.Column<int>(type: "INTEGER", nullable: false),
                    Target2 = table.Column<int>(type: "INTEGER", nullable: false),
                    Target3 = table.Column<int>(type: "INTEGER", nullable: false),
                    PointLabelId = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<ushort>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemoConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PointLabels",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Labels = table.Column<string>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<ushort>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointLabels", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemoConfigs");

            migrationBuilder.DropTable(
                name: "PointLabels");
        }
    }
}
