using FleetOpt.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOpt.Modules.Inventory.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", t =>
        {
            t.HasCheckConstraint("CK_Products_UnitWeightKg", "[UnitWeightKg] > 0");
            t.HasCheckConstraint("CK_Products_UnitVolumeM3", "[UnitVolumeM3] > 0");
        });

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Sku).HasMaxLength(50);
        builder.HasIndex(p => p.Sku).IsUnique().HasDatabaseName("UX_Products_Sku");
        builder.Property(p => p.Name).HasMaxLength(200);
        builder.Property(p => p.UnitWeightKg).HasPrecision(10, 3);
        builder.Property(p => p.UnitVolumeM3).HasPrecision(10, 4);
    }
}
