using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FleetOpt.Modules.Inventory.Features.GetWarehouseStock;

internal static class GetWarehouseStockEndpoint
{
    public static RouteGroupBuilder MapGetWarehouseStock(this RouteGroupBuilder group)
    {
        group.MapGet("/warehouses/{warehouseId:guid}/stock",
            async Task<Results<Ok<IReadOnlyList<WarehouseStockLine>>, NotFound>> (
                Guid warehouseId, GetWarehouseStockHandler handler, CancellationToken cancellationToken) =>
            {
                var stock = await handler.HandleAsync(new GetWarehouseStockQuery(warehouseId), cancellationToken);
                return stock is null ? TypedResults.NotFound() : TypedResults.Ok(stock);
            })
            .WithName("GetWarehouseStock");

        return group;
    }
}
