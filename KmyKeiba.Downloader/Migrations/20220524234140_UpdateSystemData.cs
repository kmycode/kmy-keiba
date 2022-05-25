using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class UpdateSystemData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "SystemData",
                newName: "StringValue");

            migrationBuilder.AddColumn<int>(
                name: "IntValue",
                table: "SystemData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "Key",
                table: "SystemData",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<bool>(
                name: "IsCanceled",
                table: "DownloaderTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntValue",
                table: "SystemData");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "SystemData");

            migrationBuilder.DropColumn(
                name: "IsCanceled",
                table: "DownloaderTasks");

            migrationBuilder.RenameColumn(
                name: "StringValue",
                table: "SystemData",
                newName: "Name");
        }
    }
}
