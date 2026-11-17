
namespace FleetOpt.Modules.Fleet.Domain;
internal interface ITruckRepository
{
    Task<bool> PlateNumberExistsAsync(string plateNumber, CancellationToken cancellationToken);
    void Add(Truck truck);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}