using FleetOpt.Modules.Deliveries.Domain;

namespace FleetOpt.Modules.Deliveries.Persistence;

internal sealed class DeliveryRepository(DeliveriesDbContext context) : IDeliveryRepository
{
    public void Add(Delivery delivery) => context.Deliveries.Add(delivery);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
