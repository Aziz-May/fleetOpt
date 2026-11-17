namespace FleetOpt.Modules.Inventory.Domain;

internal sealed class StockItem
{
    public Guid Id { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid ProductId { get; private set; }
    public int QuantityOnHand { get; private set; }

    private StockItem(Guid id, Guid warehouseId, Guid productId, int quantityOnHand)
    {
        Id = id;
        WarehouseId = warehouseId;
        ProductId = productId;
        QuantityOnHand = quantityOnHand;
    }

    // Only the Warehouse aggregate creates and changes stock items
    internal static StockItem Create(Guid warehouseId, Guid productId, int quantity)
        => new(Guid.CreateVersion7(), warehouseId, productId, quantity);

    internal void Increase(int quantity) => QuantityOnHand += quantity;
}
