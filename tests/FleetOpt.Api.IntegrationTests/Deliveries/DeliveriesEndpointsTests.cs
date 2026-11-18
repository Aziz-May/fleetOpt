using System.Net;
using System.Net.Http.Json;

namespace FleetOpt.Api.IntegrationTests.Deliveries;

[Collection(nameof(ApiCollection))]
public class DeliveriesEndpointsTests(FleetApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private sealed record CreatedResponse(Guid Id);
    private sealed record DeliveryLine(Guid ProductId, int Quantity);
    private sealed record DeliveryDetails(Guid Id, string CustomerName, string Priority, string Status, List<DeliveryLine> Lines);

    [Fact]
    public async Task CreateDelivery_WithKnownProducts_Returns201AndCanBeReadBack()
    {
        var productId = await CreateProductAsync("DEL-001");   // cross-module arrange: Inventory first

        var response = await _client.PostAsJsonAsync("/api/deliveries", NewDelivery(productId, quantity: 12));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var delivery = await _client.GetFromJsonAsync<DeliveryDetails>(response.Headers.Location);
        Assert.NotNull(delivery);
        Assert.Equal("High", delivery.Priority);
        Assert.Equal("Pending", delivery.Status);
        var line = Assert.Single(delivery.Lines);
        Assert.Equal(productId, line.ProductId);
        Assert.Equal(12, line.Quantity);
    }

    [Fact]
    public async Task CreateDelivery_WithUnknownProduct_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/deliveries", NewDelivery(Guid.NewGuid(), quantity: 1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateDelivery_WithPastDeadline_Returns400()
    {
        var productId = await CreateProductAsync("DEL-PAST-001");

        var response = await _client.PostAsJsonAsync("/api/deliveries",
            NewDelivery(productId, quantity: 1, deadline: DateTimeOffset.UtcNow.AddHours(-1)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static object NewDelivery(Guid productId, int quantity, DateTimeOffset? deadline = null) => new
    {
        customerName = "Monoprix Lac 2",
        destinationLatitude = 36.8425,
        destinationLongitude = 10.2683,
        priority = "High",
        deadline = deadline ?? DateTimeOffset.UtcNow.AddDays(1),
        lines = new[] { new { productId, quantity } }
    };

    private async Task<Guid> CreateProductAsync(string sku)
    {
        var response = await _client.PostAsJsonAsync("/api/inventory/products",
            new { sku, name = $"Product {sku}", unitWeightKg = 2.5, unitVolumeM3 = 0.004 });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreatedResponse>())!.Id;
    }
}
