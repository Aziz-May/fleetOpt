namespace FleetOpt.Modules.Deliveries.Domain;

internal sealed class Delivery
{
    private readonly List<DeliveryLine> _lines = [];

    public Guid Id { get; private set; }
    public string CustomerName { get; private set; }
    public decimal DestinationLatitude { get; private set; }
    public decimal DestinationLongitude { get; private set; }
    public DeliveryPriority Priority { get; private set; }
    public DateTimeOffset Deadline { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public IReadOnlyCollection<DeliveryLine> Lines => _lines.AsReadOnly();

    private Delivery(Guid id, string customerName, decimal destinationLatitude, decimal destinationLongitude,
        DeliveryPriority priority, DateTimeOffset deadline, DeliveryStatus status, DateTimeOffset createdAt)
    {
        Id = id;
        CustomerName = customerName;
        DestinationLatitude = destinationLatitude;
        DestinationLongitude = destinationLongitude;
        Priority = priority;
        Deadline = deadline;
        Status = status;
        CreatedAt = createdAt;
    }

    public static Delivery Create(
        string? customerName,
        decimal destinationLatitude,
        decimal destinationLongitude,
        DeliveryPriority priority,
        DateTimeOffset deadline,
        IReadOnlyCollection<NewDeliveryLine> lines,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required.", nameof(customerName));
        if (destinationLatitude is < -90m or > 90m)
            throw new ArgumentOutOfRangeException(nameof(destinationLatitude), "Latitude must be between -90 and 90.");
        if (destinationLongitude is < -180m or > 180m)
            throw new ArgumentOutOfRangeException(nameof(destinationLongitude), "Longitude must be between -180 and 180.");
        if (!Enum.IsDefined(priority))
            throw new ArgumentOutOfRangeException(nameof(priority), "Unknown priority.");
        if (deadline <= now)
            throw new ArgumentOutOfRangeException(nameof(deadline), "Deadline must be in the future.");
        if (lines.Count == 0)
            throw new ArgumentException("A delivery needs at least one line.", nameof(lines));
        if (lines.Any(l => l.Quantity <= 0))
            throw new ArgumentOutOfRangeException(nameof(lines), "Every line quantity must be positive.");
        if (lines.DistinctBy(l => l.ProductId).Count() != lines.Count)
            throw new ArgumentException("Each product may appear only once per delivery.", nameof(lines));

        var delivery = new Delivery(Guid.CreateVersion7(), customerName.Trim(), destinationLatitude,
            destinationLongitude, priority, deadline, DeliveryStatus.Pending, createdAt: now);

        foreach (var line in lines)
            delivery._lines.Add(DeliveryLine.Create(delivery.Id, line.ProductId, line.Quantity));

        return delivery;
    }
}
