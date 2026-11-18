using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetOpt.Modules.Inventory.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixProductColumnsAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "UnitWeightKg",
                schema: "inventory",
                table: "Products",
                type: "decimal(10,3)",
                precision: 10,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitVolumeM3",
                schema: "inventory",
                table: "Products",
                type: "decimal(10,4)",
                precision: 10,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Sku",
                schema: "inventory",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "inventory",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "UX_Products_Sku",
                schema: "inventory",
                table: "Products",
                column: "Sku",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_UnitVolumeM3",
                schema: "inventory",
                table: "Products",
                sql: "[UnitVolumeM3] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_UnitWeightKg",
                schema: "inventory",
                table: "Products",
                sql: "[UnitWeightKg] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Products_Sku",
                schema: "inventory",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_UnitVolumeM3",
                schema: "inventory",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_UnitWeightKg",
                schema: "inventory",
                table: "Products");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitWeightKg",
                schema: "inventory",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,3)",
                oldPrecision: 10,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitVolumeM3",
                schema: "inventory",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)",
                oldPrecision: 10,
                oldScale: 4);

            migrationBuilder.AlterColumn<string>(
                name: "Sku",
                schema: "inventory",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "inventory",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
