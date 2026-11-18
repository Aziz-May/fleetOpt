using System.Net;
using System.Net.Http.Json;

namespace FleetOpt.Api.IntegrationTests.Optimization;

[Collection(nameof(ApiCollection))]
public class OptimizationEndpointsTests(FleetApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private sealed record CreatedResponse(Guid Id);
    private sealed record PlanStop(int Sequence, Guid DeliveryId, decimal DistanceFromPreviousKm);
    private sealed record PlanRoute(Guid TruckId, string PlateNumber, decimal LoadWeightKg, decimal DistanceKm,
                                    decimal FuelLiters, decimal FuelCost, List<PlanStop> Stops);
    private sealed record UnassignedDelivery(Guid DeliveryId, string Reason, string Explanation);
    private sealed record PlanDetails(Guid Id, Guid WarehouseId, string Status, decimal TotalDistanceKm,
                                      decimal TotalFuelCost, List<PlanRoute> Routes, List<UnassignedDelivery> Unassigned);

    [Fact]
    public async Task GeneratePlan_ForUnknownWarehouse_Returns404()
    {
        var response = await _client.PostAsJsonAsync("/api/optimization/plans",
            new { warehouseId = Guid.NewGuid(), fuelPricePerLiter = 2.5 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GeneratePlan_EndToEnd_PutsThePendingDeliveryOnATruckRoute()
    {
        // Arrange: build a small world through the PUBLIC API of three different modules.
        await PostCreatedAsync("/api/fleet/trucks",
            new { plateNumber = "OPT-001", maxWeightKg = 5_000, maxVolumeM3 = 30, fuelConsumptionLitersPer100Km = 25 });
        var warehouseId = await PostCreatedAsync("/api/inventory/warehouses",
            new { name = "Optimization WH", latitude = 36.8065, longitude = 10.1815 });
        var productId = await PostCreatedAsync("/api/inventory/products",
            new { sku = "OPT-PRD-001", name = "Water 6x1.5L", unitWeightKg = 9.2, unitVolumeM3 = 0.012 });
        var stock = await _client.PostAsJsonAsync($"/api/inventory/warehouses/{warehouseId}/stock", new { productId, quantity = 100 });
        Assert.Equal(HttpStatusCode.NoContent, stock.StatusCode);
        var deliveryId = await PostCreatedAsync("/api/deliveries", new
        {
            customerName = "Carrefour La Marsa",
            destinationLatitude = 36.8782,
            destinationLongitude = 10.3247,
            priority = "Urgent",
            deadline = DateTimeOffset.UtcNow.AddDays(1),
            lines = new[] { new { productId, quantity = 40 } }
        });

        // Act
        var response = await _client.PostAsJsonAsync("/api/optimization/plans", new { warehouseId, fuelPricePerLiter = 2.5 });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var plan = await _client.GetFromJsonAsync<PlanDetails>(response.Headers.Location);
        Assert.NotNull(plan);
        Assert.Equal(warehouseId, plan.WarehouseId);
        Assert.Equal("Draft", plan.Status);

        // The shared test database also contains trucks and deliveries created by OTHER tests,
        // so we look for OUR delivery rather than asserting exact counts.
        var route = Assert.Single(plan.Routes, r => r.Stops.Any(s => s.DeliveryId == deliveryId));
        Assert.True(route.LoadWeightKg >= 368m);           // at least our 40 × 9.2 kg
        Assert.True(route.DistanceKm > 0);
        Assert.True(route.FuelCost > 0);
        Assert.DoesNotContain(plan.Unassigned, u => u.DeliveryId == deliveryId);
    }

    private async Task<Guid> PostCreatedAsync(string url, object body)
    {
        var response = await _client.PostAsJsonAsync(url, body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreatedResponse>())!.Id;
    }
}
