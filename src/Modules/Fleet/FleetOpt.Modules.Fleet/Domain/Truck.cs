
namespace FleetOpt.Modules.Fleet.Domain;

internal sealed class Truck
{
    public Guid Id { get; private set; }
    public string PlateNumber { get; private set; }
    public decimal MaxWeightKg { get; private set; }
    public decimal MaxVolumeM3 { get; private set; }
    public decimal FuelConsumptionLitersPer100Km { get; private set; }
    public TruckStatus Status { get; private set; }

    private Truck( Guid id, string plateNumber, decimal maxWeightKg, decimal maxVolumeM3, decimal fuelConsumptionLitersPer100Km , TruckStatus status)
    {
        Id = id ;
        PlateNumber = plateNumber;
        MaxWeightKg = maxWeightKg;
        MaxVolumeM3 = maxVolumeM3;
        FuelConsumptionLitersPer100Km = fuelConsumptionLitersPer100Km;
        Status = status ;
    }
    public static Truck Register(string? plateNumber, decimal maxWeightKg, decimal maxVolumeM3, decimal fuelConsumptionLitersPer100Km)
    {
        
        if (string.IsNullOrWhiteSpace(plateNumber))
        {
            throw new ArgumentException("Plate number cannot be empty or whitespace.", nameof(plateNumber));
        }

        if (maxWeightKg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxWeightKg), "Maximum weight must be a positive value.");
        }

        if (maxVolumeM3 <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxVolumeM3), "Maximum volume must be a positive value.");
        }

        if (fuelConsumptionLitersPer100Km <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fuelConsumptionLitersPer100Km), "Fuel consumption must be a positive value.");
        }
        var normalizedPlate = plateNumber.Trim().ToUpperInvariant();

        return new Truck(Guid.CreateVersion7(), normalizedPlate, maxWeightKg, maxVolumeM3, fuelConsumptionLitersPer100Km, TruckStatus.Available);


    }

}