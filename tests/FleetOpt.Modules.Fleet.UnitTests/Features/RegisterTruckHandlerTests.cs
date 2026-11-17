
using FleetOpt.Modules.Fleet.Domain;
using FleetOpt.Modules.Fleet.Features.RegisterTruck;
using FleetOpt.Modules.Fleet.UnitTests.Fakes;

namespace FleetOpt.Modules.Fleet.UnitTests.Features;
public class RegisterTruckHandlerTests
{
    private readonly FakeTruckRepository _truckRepository = new();
    private readonly RegisterTruckHandler _registerTruckHandler;
    public RegisterTruckHandlerTests()
    {
       
        _registerTruckHandler = new RegisterTruckHandler(_truckRepository);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_AddsTruckAndSaves()
    {
        var command = new RegisterTruckCommand("AB-123", 10_000m, 40m ,30m);

        var truckId = await _registerTruckHandler.HandleAsync(command, CancellationToken.None);
        var truck = Assert.Single(_truckRepository.Trucks); // There should be exactly one truck in the repository
        Assert.Equal(truckId , truck.Id);
        Assert.True(_truckRepository.Saved);
    }

    [Fact]
public async Task HandleAsync_WithDuplicatePlateNumber_ThrowsDuplicatePlateNumberException()
{
    // Arrange
    _truckRepository.Add(Truck.Register("AB-123", 10_000m, 40m, 30m));
    var command = new RegisterTruckCommand(" ab-123 ", 12_000m, 50m, 35m);

    // Act
    var exception = await Assert.ThrowsAsync<DuplicatePlateNumberException>(() =>
        _registerTruckHandler.HandleAsync(command, CancellationToken.None));

    // Assert
    Assert.Equal("AB-123", exception.PlateNumber);   // reported in normalized form
    Assert.False(_truckRepository.Saved);            // nothing was saved
    Assert.Single(_truckRepository.Trucks);          // the duplicate was not added
}

}