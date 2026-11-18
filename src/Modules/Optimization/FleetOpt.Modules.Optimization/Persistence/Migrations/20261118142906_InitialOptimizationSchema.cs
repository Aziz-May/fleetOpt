using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetOpt.Modules.Optimization.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialOptimizationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "optimization");

            migrationBuilder.CreateTable(
                name: "Plans",
                schema: "optimization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FuelPricePerLiter = table.Column<decimal>(type: "decimal(8,3)", precision: 8, scale: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalDistanceKm = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalFuelLiters = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalFuelCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanRoutes",
                schema: "optimization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TruckId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlateNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LoadWeightKg = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    LoadVolumeM3 = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    DistanceKm = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    FuelLiters = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    FuelCost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanRoutes_Plans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "optimization",
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanUnassignedDeliveries",
                schema: "optimization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanUnassignedDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanUnassignedDeliveries_Plans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "optimization",
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanStops",
                schema: "optimization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanRouteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    DeliveryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DistanceFromPreviousKm = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanStops", x => x.Id);
                    table.CheckConstraint("CK_PlanStops_Sequence", "[Sequence] >= 1");
                    table.ForeignKey(
                        name: "FK_PlanStops_PlanRoutes_PlanRouteId",
                        column: x => x.PlanRouteId,
                        principalSchema: "optimization",
                        principalTable: "PlanRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanRoutes_PlanId",
                schema: "optimization",
                table: "PlanRoutes",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_Warehouse_GeneratedAt",
                schema: "optimization",
                table: "Plans",
                columns: new[] { "WarehouseId", "GeneratedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_PlanStops_Route_Sequence",
                schema: "optimization",
                table: "PlanStops",
                columns: new[] { "PlanRouteId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanUnassignedDeliveries_PlanId",
                schema: "optimization",
                table: "PlanUnassignedDeliveries",
                column: "PlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanStops",
                schema: "optimization");

            migrationBuilder.DropTable(
                name: "PlanUnassignedDeliveries",
                schema: "optimization");

            migrationBuilder.DropTable(
                name: "PlanRoutes",
                schema: "optimization");

            migrationBuilder.DropTable(
                name: "Plans",
                schema: "optimization");
        }
    }
}
