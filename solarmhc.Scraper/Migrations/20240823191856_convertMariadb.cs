using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace solarmhc.Scraper.Migrations
{
    public partial class convertMariadb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Models",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BrandId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Models_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InverterTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ModelId = table.Column<int>(type: "int", nullable: false),
                    RatedCapacity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Location = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InverterTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InverterTypes_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SolarSegments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModelId = table.Column<int>(type: "int", nullable: false),
                    InstallationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Location = table.Column<int>(type: "int", nullable: false),
                    InverterTypeId = table.Column<int>(type: "int", nullable: false),
                    RatedCapacity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolarSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolarSegments_InverterTypes_InverterTypeId",
                        column: x => x.InverterTypeId,
                        principalTable: "InverterTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolarSegments_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PowerIntakes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SolarSegmentId = table.Column<int>(type: "int", nullable: false),
                    KW = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Utilization = table.Column<double>(type: "double", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerIntakes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerIntakes_SolarSegments_SolarSegmentId",
                        column: x => x.SolarSegmentId,
                        principalTable: "SolarSegments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                    { 1, 1, 2, "SolarEdge", 20000m },
                    { 2, 1, 3, "Sunny", 24000m },
                    { 3, 1, 4, "APS", 1000m },
                    { 4, 1, 5, "Huawei", 24000m },
                    { 5, 1, 6, "Fronius", 23955m }
                });

            migrationBuilder.InsertData(
                table: "SolarSegments",
                columns: new[] { "Id", "InstallationDate", "InverterTypeId", "Location", "ModelId", "Name", "Notes", "RatedCapacity" },
                values: new object[,]
                {
                    { 1, null, 1, 1, 1, "SolarEdge", "Field Segment 3", 385m },
                    { 2, null, 2, 1, 1, "Sunny", "Field Segment 4", 385m },
                    { 3, null, 3, 1, 1, "APS", "Field Segment 2", 385m },
                    { 4, null, 4, 1, 1, "Huawei", "Field Segment 1", 385m },
                    { 5, null, 5, 1, 1, "Fronius", "Field Segment 5", 385m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_InverterTypes_ModelId",
                table: "InverterTypes",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Models_BrandId",
                table: "Models",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_PowerIntakes_SolarSegmentId",
                table: "PowerIntakes",
                column: "SolarSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SolarSegments_InverterTypeId",
                table: "SolarSegments",
                column: "InverterTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SolarSegments_ModelId",
                table: "SolarSegments",
                column: "ModelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PowerIntakes");

            migrationBuilder.DropTable(
                name: "SolarSegments");

            migrationBuilder.DropTable(
                name: "InverterTypes");

            migrationBuilder.DropTable(
                name: "Models");

            migrationBuilder.DropTable(
                name: "Brands");
        }
    }
}
