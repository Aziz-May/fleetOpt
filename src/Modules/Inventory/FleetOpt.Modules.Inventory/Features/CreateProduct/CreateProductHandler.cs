using FleetOpt.Modules.Inventory.Domain;

namespace FleetOpt.Modules.Inventory.Features.CreateProduct;

internal sealed class CreateProductHandler(IProductRepository products)
{
    public async Task<Guid> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = Product.Create(command.Sku, command.Name, command.UnitWeightKg, command.UnitVolumeM3);

        if (await products.SkuExistsAsync(product.Sku, cancellationToken))   // normalized SKU, like trucks
            throw new DuplicateSkuException(product.Sku);

        products.Add(product);
        await products.SaveChangesAsync(cancellationToken);
        return product.Id;
    }
}
