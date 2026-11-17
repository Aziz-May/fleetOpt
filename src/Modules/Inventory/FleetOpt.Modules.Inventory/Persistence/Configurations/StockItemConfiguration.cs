using FleetOpt.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOpt.Modules.Inventory.Persistence.Configurations;

internal sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("StockItems", t =>
            t.HasCheckConstraint("CK_StockItems_QuantityOnHand", "[QuantityOnHand] >= 0"));

        builder.Property(s => s.Id).ValueGeneratedNever(); // we generate the Id in the domain, not the database

        // The database ALSO enforces "one line per product per warehouse"
        builder.HasIndex(s => new { s.WarehouseId, s.ProductId })
            .IsUnique()
            .HasDatabaseName("UX_StockItems_Warehouse_Product");

        // FK to Product, without a navigation property on either side
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);   // can't delete a product that still has stock
    }
}
