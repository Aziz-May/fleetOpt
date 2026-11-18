namespace FleetOpt.Modules.Optimization.Features.GenerateDeliveryPlan;

/// <summary>"Plan today's deliveries from this warehouse, assuming this fuel price."</summary>
internal sealed record GenerateDeliveryPlanCommand(Guid WarehouseId, decimal FuelPricePerLiter);

internal sealed record CreatedResponse(Guid Id);
