using FleetOpt.Modules.Inventory.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FleetOpt.Modules.Inventory.Contracts;

internal sealed class InventoryModuleApi(InventoryDbContext context) : IInventoryModuleApi
{
    public async Task<IReadOnlySet<Guid>> GetExistingProductIdsAsync(
        IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
    {
        var existing = await context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))     // → WHERE Id IN (...)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        return existing.ToHashSet();
    }

    public Task<WarehouseSnapshot?> GetWarehouseSnapshotAsync(Guid warehouseId, CancellationToken cancellationToken)
        => context.Warehouses
            .AsNoTracking()
            .Where(w => w.Id == warehouseId)
            .Select(w => new WarehouseSnapshot(
                w.Id,
                w.Name,
                w.Latitude,
                w.Longitude,
                // nested projection: EF loads the stock lines in the SAME query (LEFT JOIN)
                w.StockItems.Select(s => new StockLevel(s.ProductId, s.QuantityOnHand)).ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<ProductDimensions>> GetProductDimensionsAsync(
        IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
        => await context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))      // one query for ALL products: no N+1
            .Select(p => new ProductDimensions(p.Id, p.UnitWeightKg, p.UnitVolumeM3))
            .ToListAsync(cancellationToken);
}
