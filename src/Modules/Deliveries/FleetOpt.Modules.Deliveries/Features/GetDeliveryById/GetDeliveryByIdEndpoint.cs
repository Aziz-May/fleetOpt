using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FleetOpt.Modules.Deliveries.Features.GetDeliveryById;

internal static class GetDeliveryByIdEndpoint
{
    public static RouteGroupBuilder MapGetDeliveryById(this RouteGroupBuilder group)
    {
        group.MapGet("/{deliveryId:guid}", async Task<Results<Ok<DeliveryDetailsResponse>, NotFound>> (
            Guid deliveryId, GetDeliveryByIdHandler handler, CancellationToken cancellationToken) =>
        {
            var delivery = await handler.HandleAsync(deliveryId, cancellationToken);
            return delivery is null ? TypedResults.NotFound() : TypedResults.Ok(delivery);
        })
        .WithName("GetDeliveryById");

        return group;
    }
}
