using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddMemo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Memos",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Target1 = table.Column<short>(type: "INTEGER", nullable: false),
                    Key1 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Target2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Key2 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Target3 = table.Column<short>(type: "INTEGER", nullable: false),
                    Key3 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CourseKey = table.Column<short>(type: "INTEGER", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Memo = table.Column<string>(type: "TEXT", nullable: false),
                    Point = table.Column<short>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<ushort>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Memos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Memos_Target1_Key1",
                table: "Memos",
                columns: new[] { "Target1", "Key1" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Memos");
        }
    }
}
