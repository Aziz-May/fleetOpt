
namespace FleetOpt.Modules.Fleet.Domain;

internal sealed class Truck
{
    public Guid Id { get; private set; }
    public string PlateNumber { get; private set; }
    public decimal MaxWeightKg { get; private set; }
    public decimal MaxVolumeM3 { get; private set; }
    public decimal FuelConsumptionLitersPer100Km { get; private set; }
    public TruckStatus Status { get; private set; }

    private Truck(string plateNumber, decimal maxWeightKg, decimal maxVolumeM3, decimal fuelConsumptionLitersPer100Km)
    {
        Id = Guid.CreateVersion7();
        PlateNumber = plateNumber;
        MaxWeightKg = maxWeightKg;
        MaxVolumeM3 = maxVolumeM3;
        FuelConsumptionLitersPer100Km = fuelConsumptionLitersPer100Km;
        Status = TruckStatus.Available;
    }
    public static Truck Register(string plateNumber, decimal maxWeightKg, decimal maxVolumeM3, decimal fuelConsumptionLitersPer100Km)
    {
        var normalizedPlate = plateNumber.Trim().ToUpperInvariant();

        return new Truck(normalizedPlate, maxWeightKg, maxVolumeM3, fuelConsumptionLitersPer100Km);


    }

}