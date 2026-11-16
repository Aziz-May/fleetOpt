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
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Register_WithEmptyOrWhitespacePlateNumber_ThrowsArgumentException(string? plateNumber)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Truck.Register(plateNumber, maxWeightKg: 10_000m, maxVolumeM3: 40m, fuelConsumptionLitersPer100Km: 30m));

        Assert.Equal("plateNumber", exception.ParamName);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Register_WithNonPositiveMaxWeight_ThrowsArgumentOutOfRangeException(decimal maxWeight)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Truck.Register("AB-123", maxWeight, maxVolumeM3: 40m, fuelConsumptionLitersPer100Km: 30m));

        Assert.Equal("maxWeightKg", exception.ParamName);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Register_WithNonPositiveMaxVolume_ThrowsArgumentOutOfRangeException(decimal maxVolume)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Truck.Register("AB-123", maxWeightKg: 10_000m, maxVolumeM3: maxVolume, fuelConsumptionLitersPer100Km: 30m));

        Assert.Equal("maxVolumeM3", exception.ParamName);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Register_WithNonPositiveFuelConsumption_ThrowsArgumentOutOfRangeException(decimal fuelConsumptionLitersPer100Km)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Truck.Register("AB-123", maxWeightKg: 10_000m, maxVolumeM3: 40m, fuelConsumptionLitersPer100Km: fuelConsumptionLitersPer100Km));

        Assert.Equal("fuelConsumptionLitersPer100Km", exception.ParamName);
    }
}
