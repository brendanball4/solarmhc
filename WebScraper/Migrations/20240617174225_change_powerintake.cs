using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace solarmhc.Models.Migrations
{
    public partial class change_powerintake : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Watts",
                table: "PowerIntakes",
                newName: "KW");

            migrationBuilder.AlterColumn<double>(
                name: "Utilization",
                table: "PowerIntakes",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "KW",
                table: "PowerIntakes",
                newName: "Watts");

            migrationBuilder.AlterColumn<int>(
                name: "Utilization",
                table: "PowerIntakes",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}
