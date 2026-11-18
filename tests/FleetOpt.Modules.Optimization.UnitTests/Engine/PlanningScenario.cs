using FleetOpt.Modules.Optimization.Engine;

namespace FleetOpt.Modules.Optimization.UnitTests.Engine;

/// <summary>
/// Test Data Builder: builds a <see cref="PlanningRequest"/> step by step with sensible defaults,
/// so each test only states what matters for ITS scenario.
///
/// Geometry used in tests: the depot is at (0, 0), on the equator.
/// There, 0.01 degree of latitude or longitude ≈ 1.11 km, which makes distances easy to reason about.
/// </summary>
internal sealed class PlanningScenario
{
    public static readonly GeoPoint Depot = new(0, 0);
    public static readonly DateTimeOffset Now = new(2026, 11, 20, 8, 0, 0, TimeSpan.Zero);

    private readonly List<PlanningTruck> _trucks = [];
    private readonly List<PlanningDelivery> _deliveries = [];
    private readonly Dictionary<Guid, PlanningProduct> _products = [];
    private readonly Dictionary<Guid, int> _stock = [];
    private decimal _fuelPricePerLiter = 2.5m;

    /// <summary>Adds a product with its stock in the depot. Returns the product id.</summary>
    public Guid AddProduct(decimal unitWeightKg = 1m, decimal unitVolumeM3 = 0.001m, int stock = 1_000)
    {
        var productId = Guid.NewGuid();
        _products[productId] = new PlanningProduct(productId, unitWeightKg, unitVolumeM3);
        _stock[productId] = stock;
        return productId;
    }

    /// <summary>Adds a truck. Returns the truck id.</summary>
    public Guid AddTruck(
        decimal maxWeightKg = 1_000m,
        decimal maxVolumeM3 = 10m,
        decimal fuelLitersPer100Km = 30m,
        string plateNumber = "TRK-001")
    {
        var truckId = Guid.NewGuid();
        _trucks.Add(new PlanningTruck(truckId, plateNumber, maxWeightKg, maxVolumeM3, fuelLitersPer100Km));
        return truckId;
    }

    /// <summary>Adds a one-line delivery. Returns the delivery id.</summary>
    public Guid AddDelivery(
        Guid productId,
        int quantity,
        PlanningPriority priority = PlanningPriority.Normal,
        double hoursUntilDeadline = 24,
        double latitude = 0,
        double longitude = 0.01)
    {
        var deliveryId = Guid.NewGuid();
        _deliveries.Add(new PlanningDelivery(
            deliveryId,
            new GeoPoint(latitude, longitude),
            priority,
            Now.AddHours(hoursUntilDeadline),
            [new PlanningDeliveryLine(productId, quantity)]));
        return deliveryId;
    }

    public PlanningScenario WithFuelPrice(decimal pricePerLiter)
    {
        _fuelPricePerLiter = pricePerLiter;
        return this;
    }

    public PlanningRequest Build()
        => new(Depot, _trucks.ToList(), _deliveries.ToList(),
               new Dictionary<Guid, PlanningProduct>(_products),
               new Dictionary<Guid, int>(_stock),
               _fuelPricePerLiter);
}
