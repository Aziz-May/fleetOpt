namespace FleetOpt.Modules.Inventory.Features.GetWarehouseStock;

internal sealed record GetWarehouseStockQuery(Guid WarehouseId);

internal sealed record WarehouseStockLine(
    Guid ProductId, string Sku, string Name, int QuantityOnHand, decimal UnitWeightKg, decimal UnitVolumeM3);
