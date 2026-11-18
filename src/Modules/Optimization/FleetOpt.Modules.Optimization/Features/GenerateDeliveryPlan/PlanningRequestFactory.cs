using FleetOpt.Modules.Deliveries.Contracts;
using FleetOpt.Modules.Fleet.Contracts;
using FleetOpt.Modules.Inventory.Contracts;
using FleetOpt.Modules.Optimization.Engine;

namespace FleetOpt.Modules.Optimization.Features.GenerateDeliveryPlan;

/// <summary>
/// Translates the other modules' contract data into the engine's own input types.
/// </summary>
/// <remarks>
/// This is an <b>anti-corruption layer</b> (a term from Domain-Driven Design): the only place that knows
/// both "their" vocabulary (AvailableTruck, PendingDelivery…) and "ours" (PlanningTruck, PlanningDelivery…).
/// If Deliveries renames a field tomorrow, only this file changes; the engine and its 25 tests do not.
/// </remarks>
internal static class PlanningRequestFactory
{
    public static PlanningRequest Create(
        WarehouseSnapshot warehouse,
        IReadOnlyList<AvailableTruck> trucks,
        IReadOnlyList<PendingDelivery> pendingDeliveries,
        IReadOnlyList<ProductDimensions> products,
        decimal fuelPricePerLiter)
        => new(
            Depot: ToGeoPoint(warehouse.Latitude, warehouse.Longitude),
            Trucks: trucks
                .Select(t => new PlanningTruck(t.TruckId, t.PlateNumber, t.MaxWeightKg, t.MaxVolumeM3, t.FuelConsumptionLitersPer100Km))
                .ToList(),
            Deliveries: pendingDeliveries
                .Select(d => new PlanningDelivery(
                    d.DeliveryId,
                    ToGeoPoint(d.DestinationLatitude, d.DestinationLongitude),
                    ToPlanningPriority(d.Priority),
                    d.Deadline,
                    d.Lines.Select(l => new PlanningDeliveryLine(l.ProductId, l.Quantity)).ToList()))
                .ToList(),
            Products: products.ToDictionary(
                p => p.ProductId,
                p => new PlanningProduct(p.ProductId, p.UnitWeightKg, p.UnitVolumeM3)),
            AvailableStock: warehouse.Stock.ToDictionary(s => s.ProductId, s => s.QuantityOnHand),
            FuelPricePerLiter: fuelPricePerLiter);

    /// <summary>Coordinates are stored as decimal(9,6) in SQL; the engine's trigonometry needs double.</summary>
    private static GeoPoint ToGeoPoint(decimal latitude, decimal longitude) => new((double)latitude, (double)longitude);

    private static PlanningPriority ToPlanningPriority(PendingDeliveryPriority priority) => priority switch
    {
        PendingDeliveryPriority.Low => PlanningPriority.Low,
        PendingDeliveryPriority.Normal => PlanningPriority.Normal,
        PendingDeliveryPriority.High => PlanningPriority.High,
        PendingDeliveryPriority.Urgent => PlanningPriority.Urgent,
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, "Unknown delivery priority.")
    };
}
