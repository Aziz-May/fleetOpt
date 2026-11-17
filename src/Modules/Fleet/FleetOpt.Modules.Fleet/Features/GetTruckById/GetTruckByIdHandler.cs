using FleetOpt.Modules.Fleet.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FleetOpt.Modules.Fleet.Features.GetTruckById;

internal sealed class GetTruckByIdHandler(FleetDbContext context)
{
    public Task<TruckDetailsResponse?> HandleAsync(GetTruckByIdQuery query, CancellationToken cancellationToken)
        => context.Trucks
            .AsNoTracking()
            .Where(t => t.Id == query.TruckId)
            .Select(t => new TruckDetailsResponse(
                t.Id,
                t.PlateNumber,
                t.MaxWeightKg,
                t.MaxVolumeM3,
                t.FuelConsumptionLitersPer100Km,
                t.Status.ToString()))
            .SingleOrDefaultAsync(cancellationToken);
}
