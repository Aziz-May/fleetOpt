using System.Net;
using System.Net.Http.Json;

namespace FleetOpt.Api.IntegrationTests.Fleet;

[Collection(nameof(ApiCollection))]
public class RegisterTruckTests(FleetApiFactory factory) 
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task RegisterTruck_WithValidRequest_Returns201WithLocation()
    {
        var response = await _client.PostAsJsonAsync("/api/fleet/trucks", new
        {
            plateNumber = "INT-001",
            maxWeightKg = 10_000,
            maxVolumeM3 = 40,
            fuelConsumptionLitersPer100Km = 30
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact] 
    public async Task RegisterTruck_WithDuplicatePlateNumber_Returns409Conflict()
    {
        // First, register a truck with a specific plate number
        var initialResponse = await _client.PostAsJsonAsync("/api/fleet/trucks", new
        {
            plateNumber = "INT-002",
            maxWeightKg = 10_000,
            maxVolumeM3 = 40,
            fuelConsumptionLitersPer100Km = 30
        });

        Assert.Equal(HttpStatusCode.Created, initialResponse.StatusCode);

        // Now, attempt to register another truck with the same plate number (case-insensitive)
        var duplicateResponse = await _client.PostAsJsonAsync("/api/fleet/trucks", new
        {
            plateNumber = " int-002 ", // intentionally using different casing and whitespace
            maxWeightKg = 12_000,
            maxVolumeM3 = 50,
            fuelConsumptionLitersPer100Km = 35
        });

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        
    }
    [Fact]
        public async Task RegisterTruck_WithInvalidData_Returns400BadRequest()
        {
            var response = await _client.PostAsJsonAsync("/api/fleet/trucks", new
            {
                plateNumber = "", // Invalid: empty plate number
                maxWeightKg = -10_000, // Invalid: negative weight
                maxVolumeM3 = 0, // Invalid: zero volume
                fuelConsumptionLitersPer100Km = -5 // Invalid: negative fuel consumption
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
}
