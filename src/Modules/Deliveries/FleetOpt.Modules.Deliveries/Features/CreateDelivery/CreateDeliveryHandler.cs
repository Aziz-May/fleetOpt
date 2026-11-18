using FleetOpt.Modules.Deliveries.Domain;
using FleetOpt.Modules.Inventory.Contracts;

namespace FleetOpt.Modules.Deliveries.Features.CreateDelivery;

internal sealed class CreateDeliveryHandler(
    IDeliveryRepository deliveries,
    IInventoryModuleApi inventory,     // ← another module, through its public contract only
    TimeProvider timeProvider)         // ← "now", injectable
{
    public async Task<Guid> HandleAsync(CreateDeliveryCommand command, CancellationToken cancellationToken)
    {
        // 1. Domain rules (cheap, no I/O): fail fast before calling another module
        var delivery = Delivery.Create(
            command.CustomerName,
            command.DestinationLatitude,
            command.DestinationLongitude,
            command.Priority,
            command.Deadline,
            command.Lines ?? [],
            timeProvider.GetUtcNow());

        // 2. Cross-module rule: every product must exist in Inventory (ONE call for all lines)
        var productIds = delivery.Lines.Select(l => l.ProductId).ToList();
        var existing = await inventory.GetExistingProductIdsAsync(productIds, cancellationToken);
        var unknown = productIds.Where(id => !existing.Contains(id)).ToList();
        if (unknown.Count > 0)
            throw new UnknownProductsException(unknown);

        // 3. Persist
        deliveries.Add(delivery);
        await deliveries.SaveChangesAsync(cancellationToken);
        return delivery.Id;
    }
}
