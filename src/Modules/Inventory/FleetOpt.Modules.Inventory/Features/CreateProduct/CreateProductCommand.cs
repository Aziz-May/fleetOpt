namespace FleetOpt.Modules.Inventory.Features.CreateProduct;

internal sealed record CreateProductCommand(string? Sku, string? Name, decimal UnitWeightKg, decimal UnitVolumeM3);
