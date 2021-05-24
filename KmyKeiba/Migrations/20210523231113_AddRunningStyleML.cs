using Microsoft.EntityFrameworkCore.Migrations;

namespace KmyKeiba.Migrations
{
    public partial class AddRunningStyleML : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRunningStyleSetManually",
                table: "RaceHorses",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRunningStyleSetManually",
                table: "RaceHorses");
        }
    }
}
