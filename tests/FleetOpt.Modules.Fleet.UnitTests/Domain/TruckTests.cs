using FleetOpt.Modules.Fleet.Domain;

namespace FleetOpt.Modules.Fleet.UnitTests.Domain;

public class TruckTests
{
    [Fact]
    public void Register_WithValidData_CreatesAvailableTruck()
    {
        // Arrange + Act
        var truck = Truck.Register("AB-123", maxWeightKg: 10_000m, maxVolumeM3: 40m, fuelConsumptionLitersPer100Km: 30m);

        // Assert
        Assert.NotEqual(Guid.Empty, truck.Id);
        Assert.Equal("AB-123", truck.PlateNumber);
        Assert.Equal(10_000m, truck.MaxWeightKg);
        Assert.Equal(TruckStatus.Available, truck.Status);
    }

    [Fact]
    public void Register_WithUntrimmedLowercasePlateNumber_StoresNormalizedPlate()
    {
        // Arrange + Act
        var truck = Truck.Register(" ab-123 ", maxWeightKg: 10_000m, maxVolumeM3: 40m, fuelConsumptionLitersPer100Km: 30m);

        // Assert
        Assert.Equal("AB-123", truck.PlateNumber);
    }
}
