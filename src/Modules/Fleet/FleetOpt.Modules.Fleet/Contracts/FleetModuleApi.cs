using FleetOpt.Modules.Fleet.Domain;
using FleetOpt.Modules.Fleet.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FleetOpt.Modules.Fleet.Contracts;

/// <summary>Internal implementation of the Fleet contract: a read-only query, like any query slice.</summary>
internal sealed class FleetModuleApi(FleetDbContext context) : IFleetModuleApi
{
    public async Task<IReadOnlyList<AvailableTruck>> GetAvailableTrucksAsync(CancellationToken cancellationToken)
        => await context.Trucks
            .AsNoTracking()
            .Where(t => t.Status == TruckStatus.Available)
            .OrderBy(t => t.PlateNumber)   // stable order → the plan is reproducible
            .Select(t => new AvailableTruck(
                t.Id, t.PlateNumber, t.MaxWeightKg, t.MaxVolumeM3, t.FuelConsumptionLitersPer100Km))
            .ToListAsync(cancellationToken);
}
