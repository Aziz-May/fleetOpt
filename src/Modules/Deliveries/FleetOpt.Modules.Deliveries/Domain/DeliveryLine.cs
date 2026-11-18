namespace FleetOpt.Modules.Deliveries.Domain;

internal sealed class DeliveryLine
{
    public Guid Id { get; private set; }
    public Guid DeliveryId { get; private set; }
    public Guid ProductId { get; private set; }   // refers to Inventory: an ID only, never a Product object
    public int Quantity { get; private set; }

    private DeliveryLine(Guid id, Guid deliveryId, Guid productId, int quantity)
    {
        Id = id;
        DeliveryId = deliveryId;
        ProductId = productId;
        Quantity = quantity;
    }

    internal static DeliveryLine Create(Guid deliveryId, Guid productId, int quantity)
        => new(Guid.CreateVersion7(), deliveryId, productId, quantity);
}
