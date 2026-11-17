
namespace FleetOpt.Modules.Fleet.Features.RegisterTruck;

internal sealed record RegisterTruckCommand(
    string? PlateNumber,
    decimal MaxWeightKg,
    decimal MaxVolumeM3,
    decimal FuelConsumptionLitersPer100Km);