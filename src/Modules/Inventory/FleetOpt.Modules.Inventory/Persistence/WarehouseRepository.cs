using FleetOpt.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace FleetOpt.Modules.Inventory.Persistence;

internal sealed class WarehouseRepository(InventoryDbContext context) : IWarehouseRepository
{
    public Task<Warehouse?> GetByIdAsync(Guid warehouseId, CancellationToken cancellationToken)
        => context.Warehouses
            .Include(w => w.StockItems)            // load the WHOLE aggregate: warehouse + its stock lines
            .SingleOrDefaultAsync(w => w.Id == warehouseId, cancellationToken);

    public void Add(Warehouse warehouse) => context.Warehouses.Add(warehouse);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
