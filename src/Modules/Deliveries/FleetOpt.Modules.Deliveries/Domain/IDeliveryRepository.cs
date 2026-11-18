namespace FleetOpt.Modules.Deliveries.Domain;

internal interface IDeliveryRepository
{
    void Add(Delivery delivery);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
