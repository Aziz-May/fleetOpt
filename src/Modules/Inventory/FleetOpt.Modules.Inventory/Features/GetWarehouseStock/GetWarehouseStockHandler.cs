using FleetOpt.Modules.Inventory.Domain;
using FleetOpt.Modules.Inventory.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FleetOpt.Modules.Inventory.Features.GetWarehouseStock;

internal sealed class GetWarehouseStockHandler(InventoryDbContext context)
{
    public async Task<IReadOnlyList<WarehouseStockLine>?> HandleAsync(
        GetWarehouseStockQuery query, CancellationToken cancellationToken)
    {
        var warehouseExists = await context.Warehouses
            .AnyAsync(w => w.Id == query.WarehouseId, cancellationToken);
        if (!warehouseExists)
            return null;                                   // → 404

        return await (
            from stock in context.Set<StockItem>().AsNoTracking()
            join product in context.Products.AsNoTracking() on stock.ProductId equals product.Id
            where stock.WarehouseId == query.WarehouseId
            orderby product.Sku
            select new WarehouseStockLine(
                product.Id, product.Sku, product.Name,
                stock.QuantityOnHand, product.UnitWeightKg, product.UnitVolumeM3)
        ).ToListAsync(cancellationToken);
    }
}
