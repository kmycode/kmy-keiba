using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class UpdateIndices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Races_Key_Name_Course",
                table: "Races");

            migrationBuilder.DropIndex(
                name: "IX_Horses_Code_FatherBreedingCode_MotherBreedingCode",
                table: "Horses");

            migrationBuilder.DropIndex(
                name: "IX_BornHorses_Code",
                table: "BornHorses");

            migrationBuilder.CreateIndex(
                name: "IX_Races_Key_StartTime_Course",
                table: "Races",
                columns: new[] { "Key", "StartTime", "Course" });

            migrationBuilder.CreateIndex(
                name: "IX_Horses_Code_MFBreedingCode",
                table: "Horses",
                columns: new[] { "Code", "MFBreedingCode" });

            migrationBuilder.CreateIndex(
                name: "IX_BornHorses_Code_MFBreedingCode",
                table: "BornHorses",
                columns: new[] { "Code", "MFBreedingCode" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Races_Key_StartTime_Course",
                table: "Races");

            migrationBuilder.DropIndex(
                name: "IX_Horses_Code_MFBreedingCode",
                table: "Horses");

            migrationBuilder.DropIndex(
                name: "IX_BornHorses_Code_MFBreedingCode",
                table: "BornHorses");

            migrationBuilder.CreateIndex(
                name: "IX_Races_Key_Name_Course",
                table: "Races",
                columns: new[] { "Key", "Name", "Course" });

            migrationBuilder.CreateIndex(
                name: "IX_Horses_Code_FatherBreedingCode_MotherBreedingCode",
                table: "Horses",
                columns: new[] { "Code", "FatherBreedingCode", "MotherBreedingCode" });

            migrationBuilder.CreateIndex(
                name: "IX_BornHorses_Code",
                table: "BornHorses",
                column: "Code");
        }
    }
}
