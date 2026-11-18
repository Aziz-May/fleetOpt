using FleetOpt.Shared.Exceptions;

namespace FleetOpt.Modules.Inventory.Domain;

internal sealed class DuplicateSkuException(string sku)
    : ConflictException($"A product with SKU '{sku}' already exists.")
{
    public string Sku { get; } = sku;
}

internal sealed class WarehouseNotFoundException(Guid warehouseId)
    : NotFoundException($"Warehouse '{warehouseId}' was not found.");

internal sealed class ProductNotFoundException(Guid productId)
    : NotFoundException($"Product '{productId}' was not found.");
