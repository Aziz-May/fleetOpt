using FleetOpt.Modules.Deliveries.Contracts;
using FleetOpt.Modules.Fleet.Contracts;
using FleetOpt.Modules.Inventory.Contracts;
using FleetOpt.Modules.Optimization.Domain;

namespace FleetOpt.Modules.Optimization.UnitTests.Fakes;

// In-memory stand-ins for the other modules' contracts.
// They let us test the GenerateDeliveryPlan handler (which talks to 3 modules + a repository)
// without any database: this is the payoff of depending on interfaces, not implementations.

internal sealed class FakeFleetModuleApi : IFleetModuleApi
{
    public List<AvailableTruck> Trucks { get; } = [];

    public Task<IReadOnlyList<AvailableTruck>> GetAvailableTrucksAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<AvailableTruck>>(Trucks);
}

internal sealed class FakeInventoryModuleApi : IInventoryModuleApi
{
    public WarehouseSnapshot? Warehouse { get; set; }
    public List<ProductDimensions> Products { get; } = [];

    public Task<IReadOnlySet<Guid>> GetExistingProductIdsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlySet<Guid>>(Products.Select(p => p.ProductId).Where(productIds.Contains).ToHashSet());

    public Task<WarehouseSnapshot?> GetWarehouseSnapshotAsync(Guid warehouseId, CancellationToken cancellationToken)
        => Task.FromResult(Warehouse?.WarehouseId == warehouseId ? Warehouse : null);

    public Task<IReadOnlyList<ProductDimensions>> GetProductDimensionsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<ProductDimensions>>(Products.Where(p => productIds.Contains(p.ProductId)).ToList());
}

internal sealed class FakeDeliveriesModuleApi : IDeliveriesModuleApi
{
    public List<PendingDelivery> Pending { get; } = [];

    public Task<IReadOnlyList<PendingDelivery>> GetPendingDeliveriesAsync(CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<PendingDelivery>>(Pending);
}

internal sealed class FakePlanRepository : IPlanRepository
{
    public List<Plan> Plans { get; } = [];
    public bool Saved { get; private set; }

    public void Add(Plan plan) => Plans.Add(plan);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved = true;
        return Task.CompletedTask;
    }
}

/// <summary>A clock that always returns the same instant, so tests never depend on the real time.</summary>
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
