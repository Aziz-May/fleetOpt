using System.Net;
using System.Net.Http.Json;

namespace FleetOpt.Api.IntegrationTests.Inventory;

[Collection(nameof(ApiCollection))]
public class InventoryEndpointsTests(FleetApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private sealed record CreatedResponse(Guid Id);
    private sealed record StockLine(Guid ProductId, string Sku, string Name, int QuantityOnHand,
                                    decimal UnitWeightKg, decimal UnitVolumeM3);

    [Fact]
    public async Task CreateProduct_WithDuplicateSku_Returns409()
    {
        await CreateProductAsync("DUP-001");

        var response = await _client.PostAsJsonAsync("/api/inventory/products",
            new { sku = " dup-001 ", name = "Other", unitWeightKg = 1, unitVolumeM3 = 0.01 });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ReceiveStock_TwiceForSameProduct_AccumulatesQuantityInOneLine()
    {
        var warehouseId = await CreateWarehouseAsync("WH Accumulate");
        var productId = await CreateProductAsync("ACC-001");

        await ReceiveStockAsync(warehouseId, productId, 50);
        await ReceiveStockAsync(warehouseId, productId, 20);

        var stock = await _client.GetFromJsonAsync<List<StockLine>>($"/api/inventory/warehouses/{warehouseId}/stock");
        var line = Assert.Single(stock!);
        Assert.Equal("ACC-001", line.Sku);
        Assert.Equal(70, line.QuantityOnHand);
    }

    [Fact]
    public async Task ReceiveStock_ForUnknownWarehouse_Returns404()
    {
        var productId = await CreateProductAsync("UNK-WH-001");

        var response = await _client.PostAsJsonAsync($"/api/inventory/warehouses/{Guid.NewGuid()}/stock",
            new { productId, quantity = 5 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ReceiveStock_ForUnknownProduct_Returns404()
    {
        var warehouseId = await CreateWarehouseAsync("WH Unknown Product");

        var response = await _client.PostAsJsonAsync($"/api/inventory/warehouses/{warehouseId}/stock",
            new { productId = Guid.NewGuid(), quantity = 5 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── helpers: the "Arrange" steps, written once ──────────────────────────
    private async Task<Guid> CreateWarehouseAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/inventory/warehouses",
            new { name, latitude = 36.8065, longitude = 10.1815 });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreatedResponse>())!.Id;
    }

    private async Task<Guid> CreateProductAsync(string sku)
    {
        var response = await _client.PostAsJsonAsync("/api/inventory/products",
            new { sku, name = $"Product {sku}", unitWeightKg = 0.5, unitVolumeM3 = 0.001 });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreatedResponse>())!.Id;
    }

    private async Task ReceiveStockAsync(Guid warehouseId, Guid productId, int quantity)
    {
        var response = await _client.PostAsJsonAsync($"/api/inventory/warehouses/{warehouseId}/stock",
            new { productId, quantity });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
