using FleetOpt.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace FleetOpt.Modules.Inventory.Persistence;

internal sealed class ProductRepository(InventoryDbContext context) : IProductRepository
{
    public Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken)
        => context.Products.AnyAsync(p => p.Sku == sku, cancellationToken);

    public Task<bool> ExistsAsync(Guid productId, CancellationToken cancellationToken)
        => context.Products.AnyAsync(p => p.Id == productId, cancellationToken);

    public void Add(Product product) => context.Products.Add(product);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
