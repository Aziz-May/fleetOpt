using FleetOpt.Modules.Optimization.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetOpt.Modules.Optimization.Persistence.Configurations;

// Four small configurations for one aggregate, kept in one file because they only make sense together.
//
//   optimization.Plans 1 ──* PlanRoutes 1 ──* PlanStops
//                      1 ──* PlanUnassignedDeliveries
//
// Real FKs inside the module (cascade: deleting a plan deletes its parts).
// TruckId / DeliveryId / WarehouseId point to OTHER modules: plain columns, no FK.

internal sealed class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.FuelPricePerLiter).HasPrecision(8, 3);
        builder.Property(p => p.TotalDistanceKm).HasPrecision(10, 2);
        builder.Property(p => p.TotalFuelLiters).HasPrecision(10, 2);
        builder.Property(p => p.TotalFuelCost).HasPrecision(12, 2);

        // "Latest plans of warehouse X": equality column first, then the sort column.
        builder.HasIndex(p => new { p.WarehouseId, p.GeneratedAt }).HasDatabaseName("IX_Plans_Warehouse_GeneratedAt");

        builder.HasMany(p => p.Routes).WithOne().HasForeignKey(r => r.PlanId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Routes).HasField("_routes");

        builder.HasMany(p => p.UnassignedDeliveries).WithOne().HasForeignKey(u => u.PlanId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.UnassignedDeliveries).HasField("_unassignedDeliveries");
    }
}

internal sealed class PlanRouteConfiguration : IEntityTypeConfiguration<PlanRoute>
{
    public void Configure(EntityTypeBuilder<PlanRoute> builder)
    {
        builder.ToTable("PlanRoutes");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.PlateNumber).HasMaxLength(20);
        builder.Property(r => r.LoadWeightKg).HasPrecision(12, 3);
        builder.Property(r => r.LoadVolumeM3).HasPrecision(12, 4);
        builder.Property(r => r.DistanceKm).HasPrecision(10, 2);
        builder.Property(r => r.FuelLiters).HasPrecision(10, 2);
        builder.Property(r => r.FuelCost).HasPrecision(12, 2);

        builder.HasMany(r => r.Stops).WithOne().HasForeignKey(s => s.PlanRouteId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.Stops).HasField("_stops");
    }
}

internal sealed class PlanStopConfiguration : IEntityTypeConfiguration<PlanStop>
{
    public void Configure(EntityTypeBuilder<PlanStop> builder)
    {
        builder.ToTable("PlanStops", t => t.HasCheckConstraint("CK_PlanStops_Sequence", "[Sequence] >= 1"));
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.DistanceFromPreviousKm).HasPrecision(10, 2);

        // A route cannot have two "stop #2".
        builder.HasIndex(s => new { s.PlanRouteId, s.Sequence }).IsUnique().HasDatabaseName("UX_PlanStops_Route_Sequence");
    }
}

internal sealed class PlanUnassignedDeliveryConfiguration : IEntityTypeConfiguration<PlanUnassignedDelivery>
{
    public void Configure(EntityTypeBuilder<PlanUnassignedDelivery> builder)
    {
        builder.ToTable("PlanUnassignedDeliveries");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();
        builder.Property(u => u.Reason).HasConversion<string>().HasMaxLength(40);
        builder.Property(u => u.Explanation).HasMaxLength(1000);   // may list several product ids
    }
}
