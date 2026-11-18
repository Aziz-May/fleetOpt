using FleetOpt.Modules.Deliveries.Domain;
using FleetOpt.Modules.Deliveries.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FleetOpt.Modules.Deliveries.Contracts;

internal sealed class DeliveriesModuleApi(DeliveriesDbContext context) : IDeliveriesModuleApi
{
    public async Task<IReadOnlyList<PendingDelivery>> GetPendingDeliveriesAsync(CancellationToken cancellationToken)
    {
        // This WHERE + ORDER BY is exactly what IX_Deliveries_Status_Deadline (Status, Deadline) was designed for.
        var rows = await context.Deliveries
            .AsNoTracking()
            .Where(d => d.Status == DeliveryStatus.Pending)
            .OrderBy(d => d.Deadline)
            .ThenBy(d => d.Id)
            .Select(d => new
            {
                d.Id,
                d.CustomerName,
                d.DestinationLatitude,
                d.DestinationLongitude,
                d.Priority,
                d.Deadline,
                Lines = d.Lines.Select(l => new PendingDeliveryLine(l.ProductId, l.Quantity)).ToList()
            })
            .ToListAsync(cancellationToken);

        // Map the internal enum to the public one in C# (after the SQL query), not inside the SQL translation.
        return rows
            .Select(r => new PendingDelivery(
                r.Id, r.CustomerName, r.DestinationLatitude, r.DestinationLongitude,
                ToContract(r.Priority), r.Deadline, r.Lines))
            .ToList();
    }

    private static PendingDeliveryPriority ToContract(DeliveryPriority priority) => priority switch
    {
        DeliveryPriority.Low => PendingDeliveryPriority.Low,
        DeliveryPriority.Normal => PendingDeliveryPriority.Normal,
        DeliveryPriority.High => PendingDeliveryPriority.High,
        DeliveryPriority.Urgent => PendingDeliveryPriority.Urgent,
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, "Unknown delivery priority.")
    };
}
