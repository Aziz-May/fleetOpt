namespace FleetOpt.Modules.Deliveries.Domain;

/// <summary>Input data for one line of a new delivery (not an entity).</summary>
internal sealed record NewDeliveryLine(Guid ProductId, int Quantity);
