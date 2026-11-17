using FleetOpt.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOpt.Modules.Inventory.Persistence.Configurations;

internal sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses", t =>
        {
            t.HasCheckConstraint("CK_Warehouses_Latitude", "[Latitude] BETWEEN -90 AND 90");
            t.HasCheckConstraint("CK_Warehouses_Longitude", "[Longitude] BETWEEN -180 AND 180");
        });

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name).HasMaxLength(100);
        builder.Property(w => w.Latitude).HasPrecision(9, 6);    // ~10 cm precision
        builder.Property(w => w.Longitude).HasPrecision(9, 6);

        // One warehouse has many stock items
        builder.HasMany(w => w.StockItems)
            .WithOne()                                  // StockItem has no navigation back to Warehouse
            .HasForeignKey(s => s.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);          // delete warehouse → its stock lines go too

        // Tell EF to read/write the private list, not the read-only property
        builder.Navigation(w => w.StockItems).HasField("_stockItems");
    }
}
