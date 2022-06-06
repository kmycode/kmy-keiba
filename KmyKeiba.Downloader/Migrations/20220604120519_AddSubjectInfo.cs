using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddSubjectInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Corner1Number",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner1Position",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner2Number",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner2Position",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner3Number",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner3Position",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner4Number",
                table: "Races");

            migrationBuilder.RenameColumn(
                name: "Corner4Position",
                table: "Races",
                newName: "CornerPositionInfos");

            migrationBuilder.AddColumn<string>(
                name: "SubjectDisplayInfo",
                table: "Races",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubjectDisplayInfo",
                table: "Races");

            migrationBuilder.RenameColumn(
                name: "CornerPositionInfos",
                table: "Races",
                newName: "Corner4Position");

            migrationBuilder.AddColumn<short>(
                name: "Corner1Number",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Corner1Position",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Corner2Number",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Corner2Position",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Corner3Number",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Corner3Position",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Corner4Number",
                table: "Races",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);
        }
    }
}
