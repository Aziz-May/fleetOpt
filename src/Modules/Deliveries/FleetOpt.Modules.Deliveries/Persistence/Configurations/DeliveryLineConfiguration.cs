using FleetOpt.Modules.Deliveries.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOpt.Modules.Deliveries.Persistence.Configurations;

internal sealed class DeliveryLineConfiguration : IEntityTypeConfiguration<DeliveryLine>
{
    public void Configure(EntityTypeBuilder<DeliveryLine> builder)
    {
        builder.ToTable("DeliveryLines", t =>
            t.HasCheckConstraint("CK_DeliveryLines_Quantity", "[Quantity] > 0"));

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();   // the lesson from StockItem, applied up front

        builder.HasIndex(l => new { l.DeliveryId, l.ProductId })
            .IsUnique()
            .HasDatabaseName("UX_DeliveryLines_Delivery_Product");

        // ProductId belongs to the Inventory module (another schema).
        // Deliberately NO foreign key: modules must not be coupled at the database level.
        // Product existence is checked through IInventoryModuleApi when the delivery is created.
    }
}
