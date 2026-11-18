using FleetOpt.Modules.Deliveries.Contracts;
using FleetOpt.Modules.Fleet.Contracts;
using FleetOpt.Modules.Inventory.Contracts;
using FleetOpt.Modules.Optimization.Domain;
using FleetOpt.Modules.Optimization.Engine;
using FleetOpt.Modules.Optimization.Features.GenerateDeliveryPlan;
using FleetOpt.Modules.Optimization.UnitTests.Fakes;

namespace FleetOpt.Modules.Optimization.UnitTests.Features;

public class GenerateDeliveryPlanHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 7, 30, 0, TimeSpan.Zero);
    private static readonly Guid WarehouseId = Guid.NewGuid();
    private static readonly Guid WaterId = Guid.NewGuid();

    private readonly FakeFleetModuleApi _fleet = new();
    private readonly FakeInventoryModuleApi _inventory = new();
    private readonly FakeDeliveriesModuleApi _deliveries = new();
    private readonly FakePlanRepository _plans = new();
    private readonly GenerateDeliveryPlanHandler _handler;

    public GenerateDeliveryPlanHandlerTests()
    {
        _handler = new GenerateDeliveryPlanHandler(_fleet, _inventory, _deliveries, _plans, new FixedTimeProvider(Now));

        // Default world: one warehouse in Tunis with 100 packs of water, one truck.
        _inventory.Warehouse = new WarehouseSnapshot(WarehouseId, "Tunis Central", 36.8065m, 10.1815m,
            [new StockLevel(WaterId, 100)]);
        _inventory.Products.Add(new ProductDimensions(WaterId, UnitWeightKg: 10m, UnitVolumeM3: 0.02m));
        _fleet.Trucks.Add(new AvailableTruck(Guid.NewGuid(), "TRK-001", 1_000m, 20m, 30m));
    }

    [Fact]
    public async Task HandleAsync_WhenWarehouseDoesNotExist_ThrowsWarehouseNotFound()
    {
        var command = new GenerateDeliveryPlanCommand(Guid.NewGuid(), FuelPricePerLiter: 2.5m);

        await Assert.ThrowsAsync<WarehouseNotFoundException>(() => _handler.HandleAsync(command, CancellationToken.None));
        Assert.False(_plans.Saved);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task HandleAsync_WithNonPositiveFuelPrice_ThrowsArgumentOutOfRange(int fuelPrice)
    {
        var command = new GenerateDeliveryPlanCommand(WarehouseId, fuelPrice);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_CombinesDataFromAllModules_AndSavesADraftPlan()
    {
        var deliveryId = AddPendingDelivery(WaterId, quantity: 20, PendingDeliveryPriority.High);

        var planId = await _handler.HandleAsync(new GenerateDeliveryPlanCommand(WarehouseId, 2.5m), CancellationToken.None);

        var plan = Assert.Single(_plans.Plans);
        Assert.True(_plans.Saved);
        Assert.Equal(planId, plan.Id);
        Assert.Equal(WarehouseId, plan.WarehouseId);
        Assert.Equal(Now, plan.GeneratedAt);                 // the injected clock, not the real one
        Assert.Equal(PlanStatus.Draft, plan.Status);
        var route = Assert.Single(plan.Routes);
        Assert.Equal("TRK-001", route.PlateNumber);
        Assert.Equal(200m, route.LoadWeightKg);              // 20 × 10 kg: product data came from Inventory
        Assert.Equal(deliveryId, Assert.Single(route.Stops).DeliveryId);
        Assert.True(plan.TotalDistanceKm > 0);
        Assert.Empty(plan.UnassignedDeliveries);
    }

    [Fact]
    public async Task HandleAsync_WhenTheWarehouseLacksStock_SavesTheDeliveryAsUnassigned()
    {
        var otherProduct = Guid.NewGuid();   // known product, but never stocked in THIS warehouse
        _inventory.Products.Add(new ProductDimensions(otherProduct, 1m, 0.001m));
        var deliveryId = AddPendingDelivery(otherProduct, quantity: 5, PendingDeliveryPriority.Normal);

        await _handler.HandleAsync(new GenerateDeliveryPlanCommand(WarehouseId, 2.5m), CancellationToken.None);

        var plan = Assert.Single(_plans.Plans);
        Assert.Empty(plan.Routes);
        var unassigned = Assert.Single(plan.UnassignedDeliveries);
        Assert.Equal(deliveryId, unassigned.DeliveryId);
        Assert.Equal(UnassignedReason.InsufficientStock, unassigned.Reason);
    }

    [Fact]
    public async Task HandleAsync_MapsDeliveryPriorities_SoUrgentDeliveriesWinScarceStock()
    {
        var normal = AddPendingDelivery(WaterId, quantity: 60, PendingDeliveryPriority.Normal, hoursUntilDeadline: 2);
        var urgent = AddPendingDelivery(WaterId, quantity: 60, PendingDeliveryPriority.Urgent, hoursUntilDeadline: 8);

        await _handler.HandleAsync(new GenerateDeliveryPlanCommand(WarehouseId, 2.5m), CancellationToken.None);

        var plan = Assert.Single(_plans.Plans);
        Assert.Equal(urgent, Assert.Single(Assert.Single(plan.Routes).Stops).DeliveryId);
        Assert.Equal(normal, Assert.Single(plan.UnassignedDeliveries).DeliveryId);
    }

    private Guid AddPendingDelivery(Guid productId, int quantity, PendingDeliveryPriority priority, double hoursUntilDeadline = 6)
    {
        var deliveryId = Guid.NewGuid();
        _deliveries.Pending.Add(new PendingDelivery(
            deliveryId, "Monoprix Lac 2", 36.8425m, 10.2683m, priority, Now.AddHours(hoursUntilDeadline),
            [new PendingDeliveryLine(productId, quantity)]));
        return deliveryId;
    }
}
