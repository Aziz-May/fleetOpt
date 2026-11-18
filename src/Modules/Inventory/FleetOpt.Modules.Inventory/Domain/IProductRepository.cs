namespace FleetOpt.Modules.Inventory.Domain;

internal interface IProductRepository
{
    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid productId, CancellationToken cancellationToken);
    void Add(Product product);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
