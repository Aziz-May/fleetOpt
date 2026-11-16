using FleetOpt.Modules.Fleet.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOpt.Modules.Fleet.Persistence.Configurations;

internal sealed class TruckConfiguration : IEntityTypeConfiguration<Truck> 
{
    public void Configure(EntityTypeBuilder<Truck> builder)
    {
        builder.ToTable("Trucks" , t => {
            t.HasCheckConstraint("CK_Trucks_MaxWeightKg", "[MaxWeightKg] > 0");
            t.HasCheckConstraint("CK_Trucks_MaxVolumeM3", "[MaxVolumeM3] > 0");
            t.HasCheckConstraint("CK_Trucks_FuelConsumptionLitersPer100Km", "[FuelConsumptionLitersPer100Km] > 0");
        });

        builder.HasKey(t => t.Id);
        builder.Property(t => t.PlateNumber).IsRequired().HasMaxLength(20);
        builder.HasIndex(t => t.PlateNumber).IsUnique().HasDatabaseName("UX_Trucks_PlateNumber");
        builder.Property(t => t.MaxWeightKg).IsRequired().HasPrecision(10, 2);
        builder.Property(t => t.MaxVolumeM3).IsRequired().HasPrecision(10, 2);
        builder.Property(t => t.FuelConsumptionLitersPer100Km).IsRequired().HasPrecision(10, 2);
        builder.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(20); //stored as string so rows stay readable and reorder the neums does not corrupt data 
    }
}