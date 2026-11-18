using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetOpt.Modules.Deliveries.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialDeliveriesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "deliveries");

            migrationBuilder.CreateTable(
                name: "Deliveries",
                schema: "deliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DestinationLatitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    DestinationLongitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Deadline = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.Id);
                    table.CheckConstraint("CK_Deliveries_DestinationLatitude", "[DestinationLatitude] BETWEEN -90 AND 90");
                    table.CheckConstraint("CK_Deliveries_DestinationLongitude", "[DestinationLongitude] BETWEEN -180 AND 180");
                });

            migrationBuilder.CreateTable(
                name: "DeliveryLines",
                schema: "deliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryLines", x => x.Id);
                    table.CheckConstraint("CK_DeliveryLines_Quantity", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_DeliveryLines_Deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalSchema: "deliveries",
                        principalTable: "Deliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_Status_Deadline",
                schema: "deliveries",
                table: "Deliveries",
                columns: new[] { "Status", "Deadline" });

            migrationBuilder.CreateIndex(
                name: "UX_DeliveryLines_Delivery_Product",
                schema: "deliveries",
                table: "DeliveryLines",
                columns: new[] { "DeliveryId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryLines",
                schema: "deliveries");

            migrationBuilder.DropTable(
                name: "Deliveries",
                schema: "deliveries");
        }
    }
}
