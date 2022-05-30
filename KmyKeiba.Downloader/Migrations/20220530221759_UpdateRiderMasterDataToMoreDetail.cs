using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    public partial class UpdateRiderMasterDataToMoreDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ThirdCount",
                table: "RiderWinRates",
                newName: "ThirdTurfSteepsCount");

            migrationBuilder.RenameColumn(
                name: "SecondCount",
                table: "RiderWinRates",
                newName: "ThirdTurfCount");

            migrationBuilder.RenameColumn(
                name: "LosedCount",
                table: "RiderWinRates",
                newName: "ThirdDirtSteepsCount");

            migrationBuilder.RenameColumn(
                name: "FirstCount",
                table: "RiderWinRates",
                newName: "ThirdDirtCount");

            migrationBuilder.RenameColumn(
                name: "AllCount",
                table: "RiderWinRates",
                newName: "SecondTurfSteepsCount");

            migrationBuilder.AddColumn<short>(
                name: "AllDirtCount",
                table: "RiderWinRates",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "AllDirtSteepsCount",
                table: "RiderWinRates",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "AllTurfCount",
                table: "RiderWinRates",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "AllTurfSteepsCount",
                table: "RiderWinRates",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Distance",
                table: "RiderWinRates",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "DistanceMax",
                table: "RiderWinRates",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "FirstDirtCount",
                table: "RiderWinRates",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "FirstDirtSteepsCount",
                table: "RiderWinRates",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "FirstTurfCount",
                table: "RiderWinRates",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "FirstTurfSteepsCount",
                table: "RiderWinRates",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "SecondDirtCount",
                table: "RiderWinRates",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "SecondDirtSteepsCount",
                table: "RiderWinRates",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "SecondTurfCount",
                table: "RiderWinRates",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllDirtCount",
                table: "RiderWinRates");

            migrationBuilder.DropColumn(
                name: "AllDirtSteepsCount",
                table: "RiderWinRates");

            migrationBuilder.DropColumn(
                name: "AllTurfCount",
                table: "RiderWinRates");

            migrationBuilder.DropColumn(
                name: "AllTurfSteepsCount",
                table: "RiderWinRates");

            migrationBuilder.DropColumn(
                name: "Distance",
                table: "RiderWinRates");

            migrationBuilder.DropColumn(
                name: "DistanceMax",
                table: "RiderWinRates");

            migrationBuilder.DropColumn(
                name: "FirstDirtCount",
                table: "RiderWinRates");

            migrationBuilder.DropColumn(
                name: "FirstDirtSteepsCount",
                table: "RiderWinRates");

            migrationBuilder.DropColumn(
                name: "FirstTurfCount",
                table: "RiderWinRates");

            migrationBuilder.DropColumn(
                name: "FirstTurfSteepsCount",
                table: "RiderWinRates");

            migrationBuilder.DropColumn(
                name: "SecondDirtCount",
                table: "RiderWinRates");

            migrationBuilder.DropColumn(
                name: "SecondDirtSteepsCount",
                table: "RiderWinRates");

            migrationBuilder.DropColumn(
                name: "SecondTurfCount",
                table: "RiderWinRates");

            migrationBuilder.RenameColumn(
                name: "ThirdTurfSteepsCount",
                table: "RiderWinRates",
                newName: "ThirdCount");

            migrationBuilder.RenameColumn(
                name: "ThirdTurfCount",
                table: "RiderWinRates",
                newName: "SecondCount");

            migrationBuilder.RenameColumn(
                name: "ThirdDirtSteepsCount",
                table: "RiderWinRates",
                newName: "LosedCount");

            migrationBuilder.RenameColumn(
                name: "ThirdDirtCount",
                table: "RiderWinRates",
                newName: "FirstCount");

            migrationBuilder.RenameColumn(
                name: "SecondTurfSteepsCount",
                table: "RiderWinRates",
                newName: "AllCount");
        }
    }
}
