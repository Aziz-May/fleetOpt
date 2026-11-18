using FleetOpt.Modules.Deliveries.Domain;

namespace FleetOpt.Modules.Deliveries.Features.CreateDelivery;

internal sealed record CreateDeliveryCommand(
    string? CustomerName,
    decimal DestinationLatitude,
    decimal DestinationLongitude,
    DeliveryPriority Priority,
    DateTimeOffset Deadline,
    IReadOnlyList<NewDeliveryLine>? Lines);

internal sealed record CreatedResponse(Guid Id);
