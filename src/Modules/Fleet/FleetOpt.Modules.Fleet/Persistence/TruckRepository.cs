using FleetOpt.Modules.Fleet.Domain;
using Microsoft.EntityFrameworkCore;

namespace FleetOpt.Modules.Fleet.Persistence;
internal sealed class TruckRepository(  FleetDbContext context) : ITruckRepository
{
    public Task<bool> PlateNumberExistsAsync(string plateNumber, CancellationToken cancellationToken)
        => context.Trucks.AnyAsync(t => t.PlateNumber == plateNumber, cancellationToken);

    public void Add(Truck truck) => context.Trucks.Add(truck);

    public Task SaveChangesAsync(CancellationToken cancellationToken) 
    => context.SaveChangesAsync(cancellationToken);
}