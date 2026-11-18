using FleetOpt.Modules.Deliveries.Domain;
using Microsoft.EntityFrameworkCore;

namespace FleetOpt.Modules.Deliveries.Persistence;

internal sealed class DeliveriesDbContext(DbContextOptions<DeliveriesDbContext> options) : DbContext(options)
{
    public DbSet<Delivery> Deliveries => Set<Delivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("deliveries");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeliveriesDbContext).Assembly);
    }
}
