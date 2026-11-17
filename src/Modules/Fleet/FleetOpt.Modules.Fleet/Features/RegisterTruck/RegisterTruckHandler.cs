
using FleetOpt.Modules.Fleet.Domain;

namespace FleetOpt.Modules.Fleet.Features.RegisterTruck;

internal sealed class RegisterTruckHandler(ITruckRepository trucks)
{
    public async Task<Guid> HandleAsync(RegisterTruckCommand command, CancellationToken cancellationToken)
    {
        var truck =  Truck.Register(command.PlateNumber, command.MaxWeightKg, command.MaxVolumeM3, command.FuelConsumptionLitersPer100Km);
        if (await trucks.PlateNumberExistsAsync(truck.PlateNumber, cancellationToken))
        {
            throw new DuplicatePlateNumberException(truck.PlateNumber);
        }
        trucks.Add(truck);
        await trucks.SaveChangesAsync(cancellationToken);
        return truck.Id;
    }
}
