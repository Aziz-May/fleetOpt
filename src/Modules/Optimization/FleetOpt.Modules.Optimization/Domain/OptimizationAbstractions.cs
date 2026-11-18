using FleetOpt.Shared.Exceptions;

namespace FleetOpt.Modules.Optimization.Domain;

internal interface IPlanRepository
{
    void Add(Plan plan);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

internal sealed class WarehouseNotFoundException(Guid warehouseId)
    : NotFoundException($"Warehouse '{warehouseId}' was not found.")
{
    public Guid WarehouseId { get; } = warehouseId;
}
