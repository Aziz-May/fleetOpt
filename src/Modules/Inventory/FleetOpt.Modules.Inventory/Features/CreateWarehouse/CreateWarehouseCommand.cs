namespace FleetOpt.Modules.Inventory.Features.CreateWarehouse;

internal sealed record CreateWarehouseCommand(string? Name, decimal Latitude, decimal Longitude);
