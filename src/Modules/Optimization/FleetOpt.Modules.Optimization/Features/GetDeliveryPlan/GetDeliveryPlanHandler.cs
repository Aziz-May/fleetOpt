using FleetOpt.Modules.Optimization.Domain;
using FleetOpt.Modules.Optimization.Engine;
using FleetOpt.Modules.Optimization.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FleetOpt.Modules.Optimization.Features.GetDeliveryPlan;

internal sealed record PlanStopResponse(int Sequence, Guid DeliveryId, decimal DistanceFromPreviousKm);

internal sealed record PlanRouteResponse(
    Guid TruckId, string PlateNumber, decimal LoadWeightKg, decimal LoadVolumeM3,
    decimal DistanceKm, decimal FuelLiters, decimal FuelCost, IReadOnlyList<PlanStopResponse> Stops);

internal sealed record UnassignedDeliveryResponse(Guid DeliveryId, UnassignedReason Reason, string Explanation);

internal sealed record PlanResponse(
    Guid Id, Guid WarehouseId, DateTimeOffset GeneratedAt, PlanStatus Status, decimal FuelPricePerLiter,
    decimal TotalDistanceKm, decimal TotalFuelLiters, decimal TotalFuelCost,
    IReadOnlyList<PlanRouteResponse> Routes, IReadOnlyList<UnassignedDeliveryResponse> Unassigned);

internal sealed class GetDeliveryPlanHandler(OptimizationDbContext context)
{
    public Task<PlanResponse?> HandleAsync(Guid planId, CancellationToken cancellationToken)
        => context.Plans
            .AsNoTracking()
            .Where(p => p.Id == planId)
            .Select(p => new PlanResponse(
                p.Id, p.WarehouseId, p.GeneratedAt, p.Status, p.FuelPricePerLiter,
                p.TotalDistanceKm, p.TotalFuelLiters, p.TotalFuelCost,
                p.Routes
                    .OrderBy(r => r.PlateNumber)
                    .Select(r => new PlanRouteResponse(
                        r.TruckId, r.PlateNumber, r.LoadWeightKg, r.LoadVolumeM3, r.DistanceKm, r.FuelLiters, r.FuelCost,
                        r.Stops
                            .OrderBy(s => s.Sequence)
                            .Select(s => new PlanStopResponse(s.Sequence, s.DeliveryId, s.DistanceFromPreviousKm))
                            .ToList()))
                    .ToList(),
                p.UnassignedDeliveries
                    .Select(u => new UnassignedDeliveryResponse(u.DeliveryId, u.Reason, u.Explanation))
                    .ToList()))
            // A plan has TWO child collections (routes→stops, and unassigned). In ONE SQL query, EF would have to
            // JOIN them all, producing routes × stops × unassigned rows ("cartesian explosion").
            // AsSplitQuery() sends one SELECT per collection instead and stitches the results together in memory.
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);
}
