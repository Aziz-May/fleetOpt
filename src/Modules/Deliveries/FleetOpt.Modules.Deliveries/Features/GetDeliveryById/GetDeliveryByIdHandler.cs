using FleetOpt.Modules.Deliveries.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FleetOpt.Modules.Deliveries.Features.GetDeliveryById;

internal sealed record DeliveryLineResponse(Guid ProductId, int Quantity);

internal sealed record DeliveryDetailsResponse(
    Guid Id, string CustomerName, decimal DestinationLatitude, decimal DestinationLongitude,
    string Priority, DateTimeOffset Deadline, string Status, IReadOnlyList<DeliveryLineResponse> Lines);

internal sealed class GetDeliveryByIdHandler(DeliveriesDbContext context)
{
    public Task<DeliveryDetailsResponse?> HandleAsync(Guid deliveryId, CancellationToken cancellationToken)
        => context.Deliveries
            .AsNoTracking()
            .Where(d => d.Id == deliveryId)
            .Select(d => new DeliveryDetailsResponse(
                d.Id, d.CustomerName, d.DestinationLatitude, d.DestinationLongitude,
                d.Priority.ToString(), d.Deadline, d.Status.ToString(),
                d.Lines.Select(l => new DeliveryLineResponse(l.ProductId, l.Quantity)).ToList()))
            .SingleOrDefaultAsync(cancellationToken);
}
