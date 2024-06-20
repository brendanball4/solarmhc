using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace solarmhc.Models.Migrations
{
    public partial class Seedingv2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "InverterTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Notes",
                value: "SolarEdge");

            migrationBuilder.UpdateData(
                table: "InverterTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Notes",
                value: "Sunny");

            migrationBuilder.UpdateData(
                table: "InverterTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Notes",
                value: "APS");

            migrationBuilder.UpdateData(
                table: "InverterTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Notes",
                value: "Huawei");

            migrationBuilder.UpdateData(
                table: "InverterTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Notes",
                value: "Fronius");

            migrationBuilder.UpdateData(
                table: "SolarSegments",
                keyColumn: "Id",
                keyValue: 1,
                column: "Notes",
                value: "Field Segment 3");

            migrationBuilder.UpdateData(
                table: "SolarSegments",
                keyColumn: "Id",
                keyValue: 2,
                column: "Notes",
                value: "Field Segment 4");

            migrationBuilder.UpdateData(
                table: "SolarSegments",
                keyColumn: "Id",
                keyValue: 3,
                column: "Notes",
                value: "Field Segment 2");

            migrationBuilder.UpdateData(
                table: "SolarSegments",
                keyColumn: "Id",
                keyValue: 4,
                column: "Notes",
                value: "Field Segment 1");

            migrationBuilder.UpdateData(
                table: "SolarSegments",
                keyColumn: "Id",
                keyValue: 5,
                column: "Notes",
                value: "Field Segment 5");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "InverterTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Notes",
                value: null);

            migrationBuilder.UpdateData(
                table: "InverterTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Notes",
                value: null);

            migrationBuilder.UpdateData(
                table: "InverterTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Notes",
                value: null);

            migrationBuilder.UpdateData(
                table: "InverterTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Notes",
                value: null);

            migrationBuilder.UpdateData(
                table: "InverterTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Notes",
                value: null);

            migrationBuilder.UpdateData(
                table: "SolarSegments",
                keyColumn: "Id",
                keyValue: 1,
                column: "Notes",
                value: null);

            migrationBuilder.UpdateData(
                table: "SolarSegments",
                keyColumn: "Id",
                keyValue: 2,
                column: "Notes",
                value: null);

            migrationBuilder.UpdateData(
                table: "SolarSegments",
                keyColumn: "Id",
                keyValue: 3,
                column: "Notes",
                value: null);

            migrationBuilder.UpdateData(
                table: "SolarSegments",
                keyColumn: "Id",
                keyValue: 4,
                column: "Notes",
                value: null);

            migrationBuilder.UpdateData(
                table: "SolarSegments",
                keyColumn: "Id",
                keyValue: 5,
                column: "Notes",
                value: null);
        }
    }
}
