namespace FleetOpt.Modules.Inventory.Contracts;

/// <summary>What other modules are allowed to ask Inventory. Nothing else is visible to them.</summary>
public interface IInventoryModuleApi
{
    /// <summary>Returns which of the given product ids exist.</summary>
    Task<IReadOnlySet<Guid>> GetExistingProductIdsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken);
}
