using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FleetOpt.Modules.Fleet.Features.GetTruckById;

internal static class GetTruckByIdEndpoint
{
    public static RouteGroupBuilder MapGetTruckById(this RouteGroupBuilder group)
    {
        group.MapGet("/trucks/{truckId:guid}", async Task<Results<Ok<TruckDetailsResponse>, NotFound>> (
            Guid truckId,                         // ← from the route {truckId}
            GetTruckByIdHandler handler,          // ← from DI
            CancellationToken cancellationToken) =>
        {
            var truck = await handler.HandleAsync(new GetTruckByIdQuery(truckId), cancellationToken);

            return truck is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(truck);
        })
        .WithName("GetTruckById");

        return group;
    }
}
