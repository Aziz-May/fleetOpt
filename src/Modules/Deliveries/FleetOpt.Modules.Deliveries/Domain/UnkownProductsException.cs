namespace FleetOpt.Modules.Deliveries.Domain;

/// <summary>The request refers to products Inventory does not know (HTTP 400).</summary>
internal sealed class UnknownProductsException(IReadOnlyCollection<Guid> productIds)
    : ArgumentException($"Unknown product(s): {string.Join(", ", productIds)}.", "lines")
{
    public IReadOnlyCollection<Guid> ProductIds { get; } = productIds;
}
