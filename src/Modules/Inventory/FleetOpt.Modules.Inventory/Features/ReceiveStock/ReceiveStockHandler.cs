using FleetOpt.Modules.Inventory.Domain;

namespace FleetOpt.Modules.Inventory.Features.ReceiveStock;

internal sealed class ReceiveStockHandler(IWarehouseRepository warehouses, IProductRepository products)
{
    public async Task HandleAsync(ReceiveStockCommand command, CancellationToken cancellationToken)
    {
        var warehouse = await warehouses.GetByIdAsync(command.WarehouseId, cancellationToken)
            ?? throw new WarehouseNotFoundException(command.WarehouseId);

        if (!await products.ExistsAsync(command.ProductId, cancellationToken))
            throw new ProductNotFoundException(command.ProductId);

        warehouse.ReceiveStock(command.ProductId, command.Quantity);   // the rule lives in the aggregate
        await warehouses.SaveChangesAsync(cancellationToken);           // INSERT or UPDATE, EF decides
    }
}
