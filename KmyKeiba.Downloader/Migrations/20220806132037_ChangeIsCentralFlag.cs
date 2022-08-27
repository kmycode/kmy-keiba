using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class ChangeIsCentralFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsCentral",
                table: "Trainers",
                newName: "CentralFlag");

            migrationBuilder.RenameColumn(
                name: "IsCentral",
                table: "Riders",
                newName: "CentralFlag");

            migrationBuilder.RenameColumn(
                name: "IsCentral",
                table: "Horses",
                newName: "CentralFlag");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CentralFlag",
                table: "Trainers",
                newName: "IsCentral");

            migrationBuilder.RenameColumn(
                name: "CentralFlag",
                table: "Riders",
                newName: "IsCentral");

            migrationBuilder.RenameColumn(
                name: "CentralFlag",
                table: "Horses",
                newName: "IsCentral");
        }
    }
}
