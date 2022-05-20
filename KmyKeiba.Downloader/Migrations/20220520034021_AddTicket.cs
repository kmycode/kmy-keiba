using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddTicket : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RaceKey = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Type = table.Column<short>(type: "INTEGER", nullable: false),
                    FormType = table.Column<short>(type: "INTEGER", nullable: false),
                    Numbers1 = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Numbers2 = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Numbers3 = table.Column<byte[]>(type: "BLOB", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<ushort>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_RaceKey",
                table: "Tickets",
                column: "RaceKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");
        }
    }
}
