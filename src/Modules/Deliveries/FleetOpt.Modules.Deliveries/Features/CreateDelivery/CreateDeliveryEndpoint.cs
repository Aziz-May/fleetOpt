using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FleetOpt.Modules.Deliveries.Features.CreateDelivery;

internal static class CreateDeliveryEndpoint
{
    public static RouteGroupBuilder MapCreateDelivery(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (
            CreateDeliveryCommand command, CreateDeliveryHandler handler, CancellationToken cancellationToken) =>
        {
            var deliveryId = await handler.HandleAsync(command, cancellationToken);
            return TypedResults.Created($"/api/deliveries/{deliveryId}", new CreatedResponse(deliveryId));
        })
        .WithName("CreateDelivery");

        return group;
    }
}
