using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class ChangeStartTimeAndMemoIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Races_Key_StartTime_Course",
                table: "Races");

            migrationBuilder.DropIndex(
                name: "IX_Memos_Target1_Key1",
                table: "Memos");

            migrationBuilder.CreateIndex(
                name: "IX_Races_StartTime_Key_Course",
                table: "Races",
                columns: new[] { "StartTime", "Key", "Course" });

            migrationBuilder.CreateIndex(
                name: "IX_Memos_Point_Target1_Key1",
                table: "Memos",
                columns: new[] { "Point", "Target1", "Key1" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Races_StartTime_Key_Course",
                table: "Races");

            migrationBuilder.DropIndex(
                name: "IX_Memos_Point_Target1_Key1",
                table: "Memos");

            migrationBuilder.CreateIndex(
                name: "IX_Races_Key_StartTime_Course",
                table: "Races",
                columns: new[] { "Key", "StartTime", "Course" });

            migrationBuilder.CreateIndex(
                name: "IX_Memos_Target1_Key1",
                table: "Memos",
                columns: new[] { "Target1", "Key1" });
        }
    }
}
