namespace FleetOpt.Modules.Inventory.Contracts;

/// <summary>What other modules are allowed to ask Inventory. Nothing else is visible to them.</summary>
public interface IInventoryModuleApi
{
    /// <summary>Returns which of the given product ids exist.</summary>
    Task<IReadOnlySet<Guid>> GetExistingProductIdsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken);

    /// <summary>A warehouse's position and current stock, or <c>null</c> if the warehouse does not exist.</summary>
    Task<WarehouseSnapshot?> GetWarehouseSnapshotAsync(Guid warehouseId, CancellationToken cancellationToken);

    /// <summary>Unit weight and volume of the given products (unknown ids are simply absent from the result).</summary>
    Task<IReadOnlyList<ProductDimensions>> GetProductDimensionsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken);
}

/// <summary>A read-only picture of a warehouse at one moment ("snapshot").</summary>
public sealed record WarehouseSnapshot(
    Guid WarehouseId,
    string Name,
    decimal Latitude,
    decimal Longitude,
    IReadOnlyList<StockLevel> Stock);

public sealed record StockLevel(Guid ProductId, int QuantityOnHand);

public sealed record ProductDimensions(Guid ProductId, decimal UnitWeightKg, decimal UnitVolumeM3);
