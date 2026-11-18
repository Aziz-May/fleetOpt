using FleetOpt.Modules.Deliveries.Domain;

namespace FleetOpt.Modules.Deliveries.UnitTests.Domain;

public class DeliveryTests
{
    // A fixed "now": tests never depend on the real clock
    private static readonly DateTimeOffset Now = new(2026, 11, 18, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidData_CreatesPendingDeliveryWithLines()
    {
        var productA = Guid.NewGuid();
        var productB = Guid.NewGuid();

        var delivery = CreateDelivery(lines: [new(productA, 10), new(productB, 5)]);

        Assert.NotEqual(Guid.Empty, delivery.Id);
        Assert.Equal(DeliveryStatus.Pending, delivery.Status);
        Assert.Equal(Now, delivery.CreatedAt);
        Assert.Equal(2, delivery.Lines.Count);
        Assert.Contains(delivery.Lines, l => l.ProductId == productA && l.Quantity == 10);
    }

    [Fact]
    public void Create_WithDeadlineNotInFuture_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => CreateDelivery(deadline: Now));
        Assert.Equal("deadline", exception.ParamName);
    }

    [Fact]
    public void Create_WithoutLines_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => CreateDelivery(lines: []));
        Assert.Equal("lines", exception.ParamName);
    }

    [Fact]
    public void Create_WithSameProductTwice_ThrowsArgumentException()
    {
        var product = Guid.NewGuid();

        var exception = Assert.Throws<ArgumentException>(() =>
            CreateDelivery(lines: [new(product, 1), new(product, 2)]));
        Assert.Equal("lines", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Create_WithNonPositiveLineQuantity_ThrowsArgumentOutOfRangeException(int quantity)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateDelivery(lines: [new(Guid.NewGuid(), quantity)]));
        Assert.Equal("lines", exception.ParamName);
    }

    [Fact]
    public void Create_WithEmptyCustomerName_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => CreateDelivery(customerName: "  "));
        Assert.Equal("customerName", exception.ParamName);
    }

    private static Delivery CreateDelivery(
        string? customerName = "Carrefour La Marsa",
        DeliveryPriority priority = DeliveryPriority.Normal,
        DateTimeOffset? deadline = null,
        IReadOnlyCollection<NewDeliveryLine>? lines = null)
        => Delivery.Create(
            customerName,
            destinationLatitude: 36.8782m,
            destinationLongitude: 10.3247m,
            priority,
            deadline ?? Now.AddDays(1),
            lines ?? [new NewDeliveryLine(Guid.NewGuid(), 1)],
            now: Now);
}
