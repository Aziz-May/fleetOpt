using FleetOpt.Modules.Inventory.Domain;

namespace FleetOpt.Modules.Inventory.UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_WithUntrimmedLowercaseSku_StoresNormalizedSku()
    {
        var product = Product.Create("  wtr-500 ", "Water 0.5L", unitWeightKg: 0.52m, unitVolumeM3: 0.0006m);

        Assert.Equal("WTR-500", product.Sku);
        Assert.Equal("Water 0.5L", product.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveWeight_ThrowsArgumentOutOfRangeException(int weight)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Product.Create("SKU-1", "Item", weight, 0.01m));
        Assert.Equal("unitWeightKg", exception.ParamName);
    }
}
