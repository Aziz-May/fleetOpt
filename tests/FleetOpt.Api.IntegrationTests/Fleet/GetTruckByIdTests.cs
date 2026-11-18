using System.Net;
using System.Net.Http.Json;

namespace FleetOpt.Api.IntegrationTests.Fleet;

[Collection(nameof(ApiCollection))]
public class GetTruckByIdTests(FleetApiFactory factory) 
{
    private readonly HttpClient _client = factory.CreateClient();

    // The shape the CLIENT expects. Defined in the test on purpose: we test the JSON contract,
    // not  internal C# types.
    private sealed record TruckDetails(
        Guid Id, string PlateNumber, decimal MaxWeightKg, decimal MaxVolumeM3,
        decimal FuelConsumptionLitersPer100Km, string Status);

    [Fact]
    public async Task GetTruckById_WhenTruckExists_Returns200WithDetails()
    {
        // Arrange: create a truck through the API, like a real client
        var created = await _client.PostAsJsonAsync("/api/fleet/trucks", new
        {
            plateNumber = " get-101 ",
            maxWeightKg = 10_000,
            maxVolumeM3 = 40,
            fuelConsumptionLitersPer100Km = 30
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        // Act: follow the Location header returned by POST
        var response = await _client.GetAsync(created.Headers.Location);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var truck = await response.Content.ReadFromJsonAsync<TruckDetails>();
        Assert.NotNull(truck);
        Assert.Equal("GET-101", truck.PlateNumber);
        Assert.Equal(10_000m, truck.MaxWeightKg);
        Assert.Equal("Available", truck.Status);
    }

    [Fact]
    public async Task GetTruckById_WhenTruckDoesNotExist_Returns404()
    {
        var response = await _client.GetAsync($"/api/fleet/trucks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
