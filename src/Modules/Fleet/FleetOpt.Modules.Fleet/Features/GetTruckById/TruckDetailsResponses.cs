namespace FleetOpt.Modules.Fleet.Features.GetTruckById;

internal sealed record TruckDetailsResponse(
    Guid Id,
    string PlateNumber,
    decimal MaxWeightKg,
    decimal MaxVolumeM3,
    decimal FuelConsumptionLitersPer100Km,
    string Status);
