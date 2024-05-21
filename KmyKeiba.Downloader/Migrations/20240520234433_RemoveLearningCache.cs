using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KmyKeiba.Downloader.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLearningCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningDataCaches");

            migrationBuilder.DropColumn(
                name: "ProducingCode",
                table: "BornHorses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProducingCode",
                table: "BornHorses",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LearningDataCaches",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Cache = table.Column<string>(type: "TEXT", nullable: false),
                    CacheVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    HorseName = table.Column<string>(type: "TEXT", nullable: false),
                    RaceKey = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningDataCaches", x => x.Id);
                });
        }
    }
}
