using FleetOpt.Modules.Deliveries.Contracts;
using FleetOpt.Modules.Fleet.Contracts;
using FleetOpt.Modules.Inventory.Contracts;
using FleetOpt.Modules.Optimization.Domain;
using FleetOpt.Modules.Optimization.Engine;

namespace FleetOpt.Modules.Optimization.Features.GenerateDeliveryPlan;

/// <summary>
/// Generates a delivery plan for one warehouse: GATHER (3 modules) → TRANSLATE → COMPUTE (engine) → SAVE.
/// </summary>
/// <remarks>
/// <para>
/// <b>Read-only towards other modules.</b> The plan is a <see cref="PlanStatus.Draft"/> proposal: no stock is
/// reserved, no truck or delivery status changes. Doing that would mean writing to three other modules in one
/// operation, i.e. a transaction across several DbContexts. In a modular monolith that is solved with an
/// "approve plan" command plus events / an outbox, which is deliberately out of scope for the MVP.
/// </para>
/// <para>
/// <b>Consistency note:</b> the three reads happen one after the other, so a truck registered between two reads
/// is simply picked up by the next plan. For a proposal that is acceptable; a dispatcher can regenerate.
/// </para>
/// </remarks>
internal sealed class GenerateDeliveryPlanHandler(
    IFleetModuleApi fleet,
    IInventoryModuleApi inventory,
    IDeliveriesModuleApi deliveries,
    IPlanRepository plans,
    TimeProvider timeProvider)
{
    public async Task<Guid> HandleAsync(GenerateDeliveryPlanCommand command, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(command.FuelPricePerLiter, nameof(command.FuelPricePerLiter));

        // 1. GATHER: ask each module, through its public contract only.
        var warehouse = await inventory.GetWarehouseSnapshotAsync(command.WarehouseId, cancellationToken)
            ?? throw new WarehouseNotFoundException(command.WarehouseId);

        var trucks = await fleet.GetAvailableTrucksAsync(cancellationToken);
        var pendingDeliveries = await deliveries.GetPendingDeliveriesAsync(cancellationToken);

        var productIds = pendingDeliveries.SelectMany(d => d.Lines).Select(l => l.ProductId).Distinct().ToList();
        var products = await inventory.GetProductDimensionsAsync(productIds, cancellationToken);   // ONE call, not one per product

        // 2. TRANSLATE: their vocabulary → the engine's vocabulary.
        var request = PlanningRequestFactory.Create(warehouse, trucks, pendingDeliveries, products, command.FuelPricePerLiter);

        // 3. COMPUTE: pure, deterministic, no I/O.
        var result = DeliveryPlanner.Plan(request);

        // 4. SAVE: one aggregate, one SaveChanges, one transaction in the optimization schema.
        var plan = Plan.FromEngineResult(warehouse.WarehouseId, command.FuelPricePerLiter, timeProvider.GetUtcNow(), result);
        plans.Add(plan);
        await plans.SaveChangesAsync(cancellationToken);

        return plan.Id;
    }
}
