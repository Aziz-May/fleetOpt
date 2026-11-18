using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace FleetOpt.Modules.Optimization.Features.GetDeliveryPlan;

internal static class GetDeliveryPlanEndpoint
{
    public static RouteGroupBuilder MapGetDeliveryPlan(this RouteGroupBuilder group)
    {
        group.MapGet("/plans/{planId:guid}", async Task<Results<Ok<PlanResponse>, NotFound>> (
            Guid planId, GetDeliveryPlanHandler handler, CancellationToken cancellationToken) =>
        {
            var plan = await handler.HandleAsync(planId, cancellationToken);
            return plan is null ? TypedResults.NotFound() : TypedResults.Ok(plan);
        })
        .WithName("GetDeliveryPlan");

        return group;
    }
}
