namespace FleetOpt.Modules.Fleet.Contracts;

/// <summary>
/// What other modules are allowed to ask the Fleet module.
/// Only plain data leaves the module: never the internal <c>Truck</c> entity.
/// </summary>
public interface IFleetModuleApi
{
    /// <summary>Trucks currently available for a new delivery plan (not on route, not in maintenance).</summary>
    Task<IReadOnlyList<AvailableTruck>> GetAvailableTrucksAsync(CancellationToken cancellationToken);
}

/// <summary>A truck's planning-relevant data, as seen from outside the Fleet module.</summary>
public sealed record AvailableTruck(
    Guid TruckId,
    string PlateNumber,
    decimal MaxWeightKg,
    decimal MaxVolumeM3,
    decimal FuelConsumptionLitersPer100Km);
