using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FleetOpt.Modules.Fleet.Features.RegisterTruck;

internal sealed record RegisterTruckResponse(Guid Id);

internal static class RegisterTruckEndpoint
{
    public static RouteGroupBuilder MapRegisterTruck(this RouteGroupBuilder group)
    {
        group.MapPost("/trucks", async (
            RegisterTruckCommand command,      // ← from the JSON body
            RegisterTruckHandler handler,      // ← from DI
            CancellationToken cancellationToken) =>  // ← cancelled if the client disconnects
        {
            var truckId = await handler.HandleAsync(command, cancellationToken);
            return TypedResults.Created($"/api/fleet/trucks/{truckId}", new RegisterTruckResponse(truckId));
        })
        .WithName("RegisterTruck");

        return group;
    }
}
