using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetOpt.Modules.Fleet.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialFleetSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fleet");

            migrationBuilder.CreateTable(
                name: "Trucks",
                schema: "fleet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlateNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaxWeightKg = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxVolumeM3 = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    FuelConsumptionLitersPer100Km = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trucks", x => x.Id);
                    table.CheckConstraint("CK_Trucks_FuelConsumptionLitersPer100Km", "[FuelConsumptionLitersPer100Km] > 0");
                    table.CheckConstraint("CK_Trucks_MaxVolumeM3", "[MaxVolumeM3] > 0");
                    table.CheckConstraint("CK_Trucks_MaxWeightKg", "[MaxWeightKg] > 0");
                });

            migrationBuilder.CreateIndex(
                name: "UX_Trucks_PlateNumber",
                schema: "fleet",
                table: "Trucks",
                column: "PlateNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trucks",
                schema: "fleet");
        }
    }
}
