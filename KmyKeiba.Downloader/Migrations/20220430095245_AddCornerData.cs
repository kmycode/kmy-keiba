using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class AddCornerData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "Corner1Number",
                table: "Races",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Corner1Position",
                table: "Races",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "Corner1Result",
                table: "Races",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<short>(
                name: "Corner2Number",
                table: "Races",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Corner2Position",
                table: "Races",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "Corner2Result",
                table: "Races",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<short>(
                name: "Corner3Number",
                table: "Races",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Corner3Position",
                table: "Races",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "Corner3Result",
                table: "Races",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<short>(
                name: "Corner4Number",
                table: "Races",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Corner4Position",
                table: "Races",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "Corner4Result",
                table: "Races",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Corner1Number",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner1Position",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner1Result",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner2Number",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner2Position",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner2Result",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner3Number",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner3Position",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner3Result",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner4Number",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner4Position",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Corner4Result",
                table: "Races");
        }
    }
}
