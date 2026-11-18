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
}
