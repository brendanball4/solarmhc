using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace solarmhc.Models.Migrations
{
    public partial class Seeding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PowerIntakes_SolarSegments_ArrayNameId",
                table: "PowerIntakes");

            migrationBuilder.RenameColumn(
                name: "ArrayNameId",
                table: "PowerIntakes",
                newName: "SolarSegmentId");

            migrationBuilder.RenameIndex(
                name: "IX_PowerIntakes_ArrayNameId",
                table: "PowerIntakes",
                newName: "IX_PowerIntakes_SolarSegmentId");

            migrationBuilder.InsertData(
                table: "Brands",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "LG" },
                    { 2, "SolarEdge" },
                    { 3, "Sunny" },
                    { 4, "APS" },
                    { 5, "Huawei" },
                    { 6, "Fronius" }
                });

            migrationBuilder.InsertData(
                table: "Models",
                columns: new[] { "Id", "BrandId", "Name" },
                values: new object[,]
                {
                    { 1, 1, "N2T-A5" },
                    { 2, 2, "SE20KUS" },
                    { 3, 3, "SUNNY TRIPOWER 24000TL-US" },
                    { 4, 4, "1000YC" },
                    { 5, 5, "25KTL inverter" },
                    { 6, 6, "SYMO 24.0-3-480" }
                });

            migrationBuilder.InsertData(
                table: "InverterTypes",
                columns: new[] { "Id", "Location", "ModelId", "Notes", "RatedCapacity" },
                values: new object[,]
                {
                    { 1, 2, 2, null, 20000m },
                    { 2, 2, 3, null, 24000m },
                    { 3, 2, 4, null, 1000m },
                    { 4, 2, 5, null, 24000m },
                    { 5, 2, 6, null, 23955m }
                });

            migrationBuilder.InsertData(
                table: "SolarSegments",
                columns: new[] { "Id", "InstallationDate", "InverterTypeId", "Location", "ModelId", "Name", "Notes", "RatedCapacity" },
                values: new object[,]
                {
                    { 1, null, 1, 1, 1, "SolarEdge", null, 385m },
                    { 2, null, 2, 1, 1, "Sunny", null, 385m },
                    { 3, null, 3, 1, 1, "APS", null, 385m },
                    { 4, null, 4, 1, 1, "Huawei", null, 385m },
                    { 5, null, 5, 1, 1, "Fronius", null, 385m }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_PowerIntakes_SolarSegments_SolarSegmentId",
                table: "PowerIntakes",
                column: "SolarSegmentId",
                principalTable: "SolarSegments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PowerIntakes_SolarSegments_SolarSegmentId",
                table: "PowerIntakes");

            migrationBuilder.DeleteData(
                table: "SolarSegments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SolarSegments",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SolarSegments",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SolarSegments",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SolarSegments",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "InverterTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "InverterTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "InverterTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "InverterTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "InverterTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Models",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Models",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Models",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Models",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Models",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Models",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.RenameColumn(
                name: "SolarSegmentId",
                table: "PowerIntakes",
                newName: "ArrayNameId");

            migrationBuilder.RenameIndex(
                name: "IX_PowerIntakes_SolarSegmentId",
                table: "PowerIntakes",
                newName: "IX_PowerIntakes_ArrayNameId");

            migrationBuilder.AddForeignKey(
                name: "FK_PowerIntakes_SolarSegments_ArrayNameId",
                table: "PowerIntakes",
                column: "ArrayNameId",
                principalTable: "SolarSegments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
