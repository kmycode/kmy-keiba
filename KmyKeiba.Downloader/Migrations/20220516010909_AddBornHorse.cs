using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddBornHorse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BornHorses",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Entried = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Retired = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Born = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 72, nullable: false),
                    Sex = table.Column<short>(type: "INTEGER", nullable: false),
                    Type = table.Column<short>(type: "INTEGER", nullable: false),
                    Color = table.Column<short>(type: "INTEGER", nullable: false),
                    FatherBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    MotherBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    FFBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    FMBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    MFBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    MMBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    FFFBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    FFMBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    FMFBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    FMMBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    MFFBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    MFMBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    MMFBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    MMMBreedingCode = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    Belongs = table.Column<short>(type: "INTEGER", nullable: false),
                    TrainerCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    TrainerName = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    InviteFrom = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    ProducingCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    OwnerCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataStatus = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BornHorses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BornHorses_Code",
                table: "BornHorses",
                column: "Code");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BornHorses");
        }
    }
}
