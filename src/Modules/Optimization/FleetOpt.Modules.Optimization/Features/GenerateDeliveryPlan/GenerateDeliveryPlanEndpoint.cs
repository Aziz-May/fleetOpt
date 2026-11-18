using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FleetOpt.Modules.Optimization.Features.GenerateDeliveryPlan;

internal static class GenerateDeliveryPlanEndpoint
{
    public static RouteGroupBuilder MapGenerateDeliveryPlan(this RouteGroupBuilder group)
    {
        // POST, not GET: generating a plan CREATES a new resource (a saved plan) → a command.
        group.MapPost("/plans", async (
            GenerateDeliveryPlanCommand command, GenerateDeliveryPlanHandler handler, CancellationToken cancellationToken) =>
        {
            var planId = await handler.HandleAsync(command, cancellationToken);
            return TypedResults.Created($"/api/optimization/plans/{planId}", new CreatedResponse(planId));
        })
        .WithName("GenerateDeliveryPlan");

        return group;
    }
}
