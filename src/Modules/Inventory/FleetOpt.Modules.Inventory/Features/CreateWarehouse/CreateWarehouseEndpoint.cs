using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FleetOpt.Modules.Inventory.Features.CreateWarehouse;

internal static class CreateWarehouseEndpoint
{
    public static RouteGroupBuilder MapCreateWarehouse(this RouteGroupBuilder group)
    {
        group.MapPost("/warehouses", async (
            CreateWarehouseCommand command, CreateWarehouseHandler handler, CancellationToken cancellationToken) =>
        {
            var warehouseId = await handler.HandleAsync(command, cancellationToken);
            return TypedResults.Created($"/api/inventory/warehouses/{warehouseId}/stock", new CreatedResponse(warehouseId));
        })
        .WithName("CreateWarehouse");

        return group;
    }
}
