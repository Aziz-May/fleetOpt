using FleetOpt.Modules.Fleet.Domain;

namespace FleetOpt.Modules.Fleet.UnitTests.Fakes;

internal sealed class FakeTruckRepository : ITruckRepository
{
    // The "database": just a list in memory
    public List<Truck> Trucks { get; } = [];

    // Lets tests check whether the handler saved
    public bool Saved { get; private set; }

    public Task<bool> PlateNumberExistsAsync(string plateNumber, CancellationToken cancellationToken)
        => Task.FromResult(Trucks.Any(t => t.PlateNumber == plateNumber));

    public void Add(Truck truck) => Trucks.Add(truck);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved = true;
        return Task.CompletedTask;
    }
}
