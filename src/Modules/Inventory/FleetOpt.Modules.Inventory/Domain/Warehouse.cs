namespace FleetOpt.Modules.Inventory.Domain;

internal sealed class Warehouse
{
    private readonly List<StockItem> _stockItems = [];   // the real, private, modifiable list

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }

    // the outside world can READ the stock, never Add/Remove directly
    public IReadOnlyCollection<StockItem> StockItems => _stockItems.AsReadOnly();

    private Warehouse(Guid id, string name, decimal latitude, decimal longitude)
    {
        Id = id;
        Name = name;
        Latitude = latitude;
        Longitude = longitude;
    }

    public static Warehouse Create(string? name, decimal latitude, decimal longitude)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Warehouse name is required.", nameof(name));
        if (latitude is < -90m or > 90m)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
        if (longitude is < -180m or > 180m)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");

        return new Warehouse(Guid.CreateVersion7(), name.Trim(), latitude, longitude);
    }

    public void ReceiveStock(Guid productId, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        var existing = _stockItems.SingleOrDefault(s => s.ProductId == productId);
        if (existing is null)
            _stockItems.Add(StockItem.Create(Id, productId, quantity));
        else
            existing.Increase(quantity);
    }
}
