using FleetOpt.Modules.Optimization.Domain;

namespace FleetOpt.Modules.Optimization.Persistence;

internal sealed class PlanRepository(OptimizationDbContext context) : IPlanRepository
{
    // Adding the root is enough: EF follows the Routes → Stops and UnassignedDeliveries collections
    // and INSERTs the whole graph in one SaveChanges (one transaction).
    public void Add(Plan plan) => context.Plans.Add(plan);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
