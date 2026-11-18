using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FleetOpt.Modules.Inventory.Features.ReceiveStock;

internal static class ReceiveStockEndpoint
{
    public static RouteGroupBuilder MapReceiveStock(this RouteGroupBuilder group)
    {
        group.MapPost("/warehouses/{warehouseId:guid}/stock", async (
            Guid warehouseId,                    // ← route
            ReceiveStockRequest request,         // ← body
            ReceiveStockHandler handler,         // ← DI
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(
                new ReceiveStockCommand(warehouseId, request.ProductId, request.Quantity), cancellationToken);

            return TypedResults.NoContent();     // 204: done, nothing to return
        })
        .WithName("ReceiveStock");

        return group;
    }
}
