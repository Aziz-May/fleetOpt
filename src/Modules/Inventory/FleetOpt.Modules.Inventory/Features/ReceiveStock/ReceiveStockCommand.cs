namespace FleetOpt.Modules.Inventory.Features.ReceiveStock;

// What the client sends in the BODY (the warehouse comes from the URL)
internal sealed record ReceiveStockRequest(Guid ProductId, int Quantity);

// What the handler needs: route + body combined
internal sealed record ReceiveStockCommand(Guid WarehouseId, Guid ProductId, int Quantity);
