using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FleetOpt.Modules.Inventory.Features.CreateProduct;

internal static class CreateProductEndpoint
{
    public static RouteGroupBuilder MapCreateProduct(this RouteGroupBuilder group)
    {
        group.MapPost("/products", async (
            CreateProductCommand command, CreateProductHandler handler, CancellationToken cancellationToken) =>
        {
            var productId = await handler.HandleAsync(command, cancellationToken);
            return TypedResults.Created($"/api/inventory/products/{productId}", new CreatedResponse(productId));
        })
        .WithName("CreateProduct");

        return group;
    }
}
