using FleetOpt.Modules.Deliveries.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOpt.Modules.Deliveries.Persistence.Configurations;

internal sealed class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.ToTable("Deliveries", t =>
        {
            t.HasCheckConstraint("CK_Deliveries_DestinationLatitude", "[DestinationLatitude] BETWEEN -90 AND 90");
            t.HasCheckConstraint("CK_Deliveries_DestinationLongitude", "[DestinationLongitude] BETWEEN -180 AND 180");
        });

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();
        builder.Property(d => d.CustomerName).HasMaxLength(200);
        builder.Property(d => d.DestinationLatitude).HasPrecision(9, 6);
        builder.Property(d => d.DestinationLongitude).HasPrecision(9, 6);
        builder.Property(d => d.Priority).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);

        // Built for the optimizer's query: "WHERE Status = 'Pending' ORDER BY Deadline"
        builder.HasIndex(d => new { d.Status, d.Deadline })
            .HasDatabaseName("IX_Deliveries_Status_Deadline");

        builder.HasMany(d => d.Lines)
            .WithOne()
            .HasForeignKey(l => l.DeliveryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.Lines).HasField("_lines");
    }
}
