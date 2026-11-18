namespace FleetOpt.Modules.Inventory.Domain;

internal interface IWarehouseRepository
{
    Task<Warehouse?> GetByIdAsync(Guid warehouseId, CancellationToken cancellationToken);
    void Add(Warehouse warehouse);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
