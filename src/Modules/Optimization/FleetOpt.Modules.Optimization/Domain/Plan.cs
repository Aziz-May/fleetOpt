using FleetOpt.Modules.Optimization.Engine;

namespace FleetOpt.Modules.Optimization.Domain;

/// <summary>Lifecycle of a plan. The MVP only produces proposals; "Approved/Executed" would come later.</summary>
internal enum PlanStatus
{
    /// <summary>A proposal: nothing has been changed in Fleet, Inventory or Deliveries yet.</summary>
    Draft
}

/// <summary>
/// A generated delivery plan, saved so it can be consulted later (aggregate root).
/// </summary>
/// <remarks>
/// <para>
/// <b>Engine result vs saved plan.</b> <see cref="DeliveryPlan"/> (engine) is a pure in-memory calculation result.
/// <see cref="Plan"/> is the persisted record of that result: when it was generated, for which warehouse,
/// at which fuel price. Keeping them separate means the engine never knows about EF Core.
/// </para>
/// <para>
/// <b>Snapshots, not references.</b> Routes store the truck's plate number, and totals are stored on the plan.
/// A plan is a historical document: if a truck's plate changes next month, last week's plan must still show
/// what was planned then. It also means reading a plan needs no call to the Fleet module.
/// </para>
/// </remarks>
internal sealed class Plan
{
    private readonly List<PlanRoute> _routes = [];
    private readonly List<PlanUnassignedDelivery> _unassignedDeliveries = [];

    public Guid Id { get; private set; }
    public Guid WarehouseId { get; private set; }
    public DateTimeOffset GeneratedAt { get; private set; }
    public decimal FuelPricePerLiter { get; private set; }
    public PlanStatus Status { get; private set; }
    public decimal TotalDistanceKm { get; private set; }
    public decimal TotalFuelLiters { get; private set; }
    public decimal TotalFuelCost { get; private set; }

    public IReadOnlyCollection<PlanRoute> Routes => _routes.AsReadOnly();
    public IReadOnlyCollection<PlanUnassignedDelivery> UnassignedDeliveries => _unassignedDeliveries.AsReadOnly();

    private Plan(Guid id, Guid warehouseId, DateTimeOffset generatedAt, decimal fuelPricePerLiter, PlanStatus status,
        decimal totalDistanceKm, decimal totalFuelLiters, decimal totalFuelCost)
    {
        Id = id;
        WarehouseId = warehouseId;
        GeneratedAt = generatedAt;
        FuelPricePerLiter = fuelPricePerLiter;
        Status = status;
        TotalDistanceKm = totalDistanceKm;
        TotalFuelLiters = totalFuelLiters;
        TotalFuelCost = totalFuelCost;
    }

    /// <summary>Records the result of one engine run as a draft plan.</summary>
    public static Plan FromEngineResult(
        Guid warehouseId, decimal fuelPricePerLiter, DateTimeOffset generatedAt, DeliveryPlan result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var plan = new Plan(
            Guid.CreateVersion7(), warehouseId, generatedAt, fuelPricePerLiter, PlanStatus.Draft,
            ToKm(result.TotalDistanceKm), ToKm(result.TotalFuelLiters), result.TotalFuelCost);

        foreach (var route in result.Routes)
            plan._routes.Add(PlanRoute.From(plan.Id, route));

        foreach (var unassigned in result.Unassigned)
            plan._unassignedDeliveries.Add(PlanUnassignedDelivery.From(plan.Id, unassigned));

        return plan;
    }

    /// <summary>The engine computes distances in double (trigonometry); the database stores decimal(…, 2).</summary>
    internal static decimal ToKm(double value) => Math.Round((decimal)value, 2);
}

/// <summary>One truck's route inside a plan.</summary>
internal sealed class PlanRoute
{
    private readonly List<PlanStop> _stops = [];

    public Guid Id { get; private set; }
    public Guid PlanId { get; private set; }
    public Guid TruckId { get; private set; }          // reference to Fleet: id only, no FK (other module)
    public string PlateNumber { get; private set; }    // snapshot, see Plan remarks
    public decimal LoadWeightKg { get; private set; }
    public decimal LoadVolumeM3 { get; private set; }
    public decimal DistanceKm { get; private set; }
    public decimal FuelLiters { get; private set; }
    public decimal FuelCost { get; private set; }
    public IReadOnlyCollection<PlanStop> Stops => _stops.AsReadOnly();

    private PlanRoute(Guid id, Guid planId, Guid truckId, string plateNumber, decimal loadWeightKg,
        decimal loadVolumeM3, decimal distanceKm, decimal fuelLiters, decimal fuelCost)
    {
        Id = id;
        PlanId = planId;
        TruckId = truckId;
        PlateNumber = plateNumber;
        LoadWeightKg = loadWeightKg;
        LoadVolumeM3 = loadVolumeM3;
        DistanceKm = distanceKm;
        FuelLiters = fuelLiters;
        FuelCost = fuelCost;
    }

    internal static PlanRoute From(Guid planId, PlannedRoute route)
    {
        var planRoute = new PlanRoute(
            Guid.CreateVersion7(), planId, route.TruckId, route.PlateNumber, route.LoadWeightKg, route.LoadVolumeM3,
            Plan.ToKm(route.DistanceKm), Plan.ToKm(route.FuelLiters), route.FuelCost);

        foreach (var stop in route.Stops)
            planRoute._stops.Add(new PlanStop(
                Guid.CreateVersion7(), planRoute.Id, stop.Sequence, stop.DeliveryId, Plan.ToKm(stop.DistanceFromPreviousKm)));

        return planRoute;
    }
}

/// <summary>One stop of a route, in driving order.</summary>
internal sealed class PlanStop(Guid id, Guid planRouteId, int sequence, Guid deliveryId, decimal distanceFromPreviousKm)
{
    public Guid Id { get; private set; } = id;
    public Guid PlanRouteId { get; private set; } = planRouteId;
    public int Sequence { get; private set; } = sequence;
    public Guid DeliveryId { get; private set; } = deliveryId;   // reference to Deliveries: id only, no FK
    public decimal DistanceFromPreviousKm { get; private set; } = distanceFromPreviousKm;
}

/// <summary>A delivery the engine could not plan, with the reason, so a dispatcher can act on it.</summary>
internal sealed class PlanUnassignedDelivery(Guid id, Guid planId, Guid deliveryId, UnassignedReason reason, string explanation)
{
    public Guid Id { get; private set; } = id;
    public Guid PlanId { get; private set; } = planId;
    public Guid DeliveryId { get; private set; } = deliveryId;
    public UnassignedReason Reason { get; private set; } = reason;
    public string Explanation { get; private set; } = explanation;

    internal static PlanUnassignedDelivery From(Guid planId, UnassignedDelivery unassigned)
        => new(Guid.CreateVersion7(), planId, unassigned.DeliveryId, unassigned.Reason, unassigned.Explanation);
}
