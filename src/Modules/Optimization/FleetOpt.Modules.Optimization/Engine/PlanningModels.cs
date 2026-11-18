namespace FleetOpt.Modules.Optimization.Engine;

// =====================================================================================
//  INPUT: everything the engine needs to know, as plain immutable data.
//
//  These types belong to the ENGINE, not to Fleet / Inventory / Deliveries.
//  The module code (later) translates other modules' contracts into these records.
//  Result: the engine never depends on EF Core, HTTP or another module, so it can be
//  tested with plain objects in milliseconds.
// =====================================================================================

/// <summary>A position on Earth, in decimal degrees.</summary>
internal readonly record struct GeoPoint(double Latitude, double Longitude);

/// <summary>How urgent a delivery is. Higher value = served first.</summary>
internal enum PlanningPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}

/// <summary>A truck that can be used in this plan.</summary>
/// <param name="FuelLitersPer100Km">Fuel consumption, used to estimate fuel and cost per route.</param>
internal sealed record PlanningTruck(
    Guid TruckId,
    string PlateNumber,
    decimal MaxWeightKg,
    decimal MaxVolumeM3,
    decimal FuelLitersPer100Km);

/// <summary>Physical characteristics of one unit of a product.</summary>
internal sealed record PlanningProduct(Guid ProductId, decimal UnitWeightKg, decimal UnitVolumeM3);

/// <summary>"Deliver <paramref name="Quantity"/> units of product X".</summary>
internal sealed record PlanningDeliveryLine(Guid ProductId, int Quantity);

/// <summary>A delivery waiting to be planned.</summary>
internal sealed record PlanningDelivery(
    Guid DeliveryId,
    GeoPoint Destination,
    PlanningPriority Priority,
    DateTimeOffset Deadline,
    IReadOnlyList<PlanningDeliveryLine> Lines);

/// <summary>The complete input of one planning run, for ONE warehouse (the depot).</summary>
/// <param name="Depot">Where every route starts and ends (the warehouse).</param>
/// <param name="Products">Product characteristics, indexed by product id.</param>
/// <param name="AvailableStock">Units available in the depot, indexed by product id.</param>
/// <param name="FuelPricePerLiter">Used to turn liters into money.</param>
internal sealed record PlanningRequest(
    GeoPoint Depot,
    IReadOnlyList<PlanningTruck> Trucks,
    IReadOnlyList<PlanningDelivery> Deliveries,
    IReadOnlyDictionary<Guid, PlanningProduct> Products,
    IReadOnlyDictionary<Guid, int> AvailableStock,
    decimal FuelPricePerLiter);

// =====================================================================================
//  OUTPUT: the plan.
// =====================================================================================

/// <summary>Why a delivery could not be planned. Every unassigned delivery gets exactly one reason.</summary>
internal enum UnassignedReason
{
    /// <summary>A line refers to a product the engine has no data for (weight/volume unknown).</summary>
    UnknownProduct,

    /// <summary>Not enough units left in the depot (after higher-priority deliveries took theirs).</summary>
    InsufficientStock,

    /// <summary>The delivery alone is heavier or bigger than the largest truck: it can never fit.</summary>
    ExceedsLargestTruckCapacity,

    /// <summary>It would fit in an empty truck, but every truck is already too full.</summary>
    NoRemainingTruckCapacity
}

internal sealed record UnassignedDelivery(Guid DeliveryId, UnassignedReason Reason, string Explanation);

/// <summary>One stop of a route, in driving order (Sequence starts at 1).</summary>
internal sealed record PlannedStop(int Sequence, Guid DeliveryId, double DistanceFromPreviousKm);

/// <summary>The work of one truck: its stops in order, its load, and its estimated cost.</summary>
/// <param name="DistanceKm">Depot → all stops → back to depot.</param>
internal sealed record PlannedRoute(
    Guid TruckId,
    string PlateNumber,
    IReadOnlyList<PlannedStop> Stops,
    decimal LoadWeightKg,
    decimal LoadVolumeM3,
    double DistanceKm,
    double FuelLiters,
    decimal FuelCost);

internal sealed record DeliveryPlan(
    IReadOnlyList<PlannedRoute> Routes,
    IReadOnlyList<UnassignedDelivery> Unassigned)
{
    public double TotalDistanceKm => Math.Round(Routes.Sum(r => r.DistanceKm), 2);
    public double TotalFuelLiters => Math.Round(Routes.Sum(r => r.FuelLiters), 2);
    public decimal TotalFuelCost => Routes.Sum(r => r.FuelCost);
}
