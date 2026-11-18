using FleetOpt.Modules.Inventory.Domain;

namespace FleetOpt.Modules.Inventory.Features.CreateWarehouse;

internal sealed class CreateWarehouseHandler(IWarehouseRepository warehouses)
{
    public async Task<Guid> HandleAsync(CreateWarehouseCommand command, CancellationToken cancellationToken)
    {
        var warehouse = Warehouse.Create(command.Name, command.Latitude, command.Longitude);
        warehouses.Add(warehouse);
        await warehouses.SaveChangesAsync(cancellationToken);
        return warehouse.Id;
    }
}
