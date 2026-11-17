using FleetOpt.Modules.Inventory.Domain;

namespace FleetOpt.Modules.Inventory.UnitTests.Domain;

public class WarehouseTests
{
    [Fact]
    public void Create_WithValidData_CreatesWarehouseWithoutStock()
    {
        var warehouse = Warehouse.Create("Tunis Central", latitude: 36.8065m, longitude: 10.1815m);

        Assert.NotEqual(Guid.Empty, warehouse.Id);
        Assert.Equal("Tunis Central", warehouse.Name);
        Assert.Empty(warehouse.StockItems);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithEmptyName_ThrowsArgumentException(string? name)
    {
        var exception = Assert.Throws<ArgumentException>(() => CreateWarehouse(name: name));
        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData(-90.1)]
    [InlineData(90.1)]
    public void Create_WithLatitudeOutOfRange_ThrowsArgumentOutOfRangeException(double latitude)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => CreateWarehouse(latitude: (decimal)latitude));
        Assert.Equal("latitude", exception.ParamName);
    }

    [Fact]
    public void ReceiveStock_ForNewProduct_AddsStockItem()
    {
        var warehouse = CreateWarehouse();
        var productId = Guid.NewGuid();

        warehouse.ReceiveStock(productId, quantity: 50);

        var item = Assert.Single(warehouse.StockItems);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(50, item.QuantityOnHand);
    }

    [Fact]
    public void ReceiveStock_ForExistingProduct_IncreasesQuantityInsteadOfAddingLine()
    {
        var warehouse = CreateWarehouse();
        var productId = Guid.NewGuid();

        warehouse.ReceiveStock(productId, quantity: 50);
        warehouse.ReceiveStock(productId, quantity: 20);

        var item = Assert.Single(warehouse.StockItems);   // still ONE line
        Assert.Equal(70, item.QuantityOnHand);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void ReceiveStock_WithNonPositiveQuantity_ThrowsArgumentOutOfRangeException(int quantity)
    {
        var warehouse = CreateWarehouse();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => warehouse.ReceiveStock(Guid.NewGuid(), quantity));
        Assert.Equal("quantity", exception.ParamName);
    }

    private static Warehouse CreateWarehouse(string? name = "Main", decimal latitude = 36.8m, decimal longitude = 10.18m)
        => Warehouse.Create(name, latitude, longitude);
}
