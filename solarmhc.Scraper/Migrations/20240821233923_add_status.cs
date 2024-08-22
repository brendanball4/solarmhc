using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace solarmhc.Scraper.Migrations
{
    public partial class add_status : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "PowerIntakes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "PowerIntakes");
        }
    }
}
