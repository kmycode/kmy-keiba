using Microsoft.EntityFrameworkCore.Migrations;

namespace KmyKeiba.Migrations
{
    public partial class AddFrameNumberOdds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FrameNumber3",
                table: "Refunds",
                newName: "Frame2Number3");

            migrationBuilder.RenameColumn(
                name: "FrameNumber2",
                table: "Refunds",
                newName: "Frame2Number2");

            migrationBuilder.RenameColumn(
                name: "FrameNumber1",
                table: "Refunds",
                newName: "Frame2Number1");

            migrationBuilder.AddColumn<short>(
                name: "Frame1Number1",
                table: "Refunds",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Frame1Number2",
                table: "Refunds",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Frame1Number3",
                table: "Refunds",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Frame1Number1",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "Frame1Number2",
                table: "Refunds");

            migrationBuilder.DropColumn(
                name: "Frame1Number3",
                table: "Refunds");

            migrationBuilder.RenameColumn(
                name: "Frame2Number3",
                table: "Refunds",
                newName: "FrameNumber3");

            migrationBuilder.RenameColumn(
                name: "Frame2Number2",
                table: "Refunds",
                newName: "FrameNumber2");

            migrationBuilder.RenameColumn(
                name: "Frame2Number1",
                table: "Refunds",
                newName: "FrameNumber1");
        }
    }
}
