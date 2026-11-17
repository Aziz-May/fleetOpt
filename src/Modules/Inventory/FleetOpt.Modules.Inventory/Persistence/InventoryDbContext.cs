using FleetOpt.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace FleetOpt.Modules.Inventory.Persistence;

internal sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Product> Products => Set<Product>();
    // No DbSet<StockItem>: stock is reached THROUGH its Warehouse (aggregate root).
    // Read-only queries can still use Set<StockItem>() when needed.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("inventory");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}
