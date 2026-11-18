namespace FleetOpt.Modules.Deliveries.Contracts;

/// <summary>What other modules are allowed to ask the Deliveries module.</summary>
public interface IDeliveriesModuleApi
{
    /// <summary>All deliveries still waiting to be planned, earliest deadline first.</summary>
    Task<IReadOnlyList<PendingDelivery>> GetPendingDeliveriesAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Public mirror of the internal <c>DeliveryPriority</c>. Contracts never expose internal types,
/// so the module can rename or extend its own enum without breaking other modules.
/// </summary>
public enum PendingDeliveryPriority
{
    Low,
    Normal,
    High,
    Urgent
}

public sealed record PendingDelivery(
    Guid DeliveryId,
    string CustomerName,
    decimal DestinationLatitude,
    decimal DestinationLongitude,
    PendingDeliveryPriority Priority,
    DateTimeOffset Deadline,
    IReadOnlyList<PendingDeliveryLine> Lines);

public sealed record PendingDeliveryLine(Guid ProductId, int Quantity);
