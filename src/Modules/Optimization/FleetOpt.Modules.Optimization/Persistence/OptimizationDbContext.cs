using FleetOpt.Modules.Optimization.Domain;
using Microsoft.EntityFrameworkCore;

namespace FleetOpt.Modules.Optimization.Persistence;

internal sealed class OptimizationDbContext(DbContextOptions<OptimizationDbContext> options) : DbContext(options)
{
    public DbSet<Plan> Plans => Set<Plan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("optimization");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OptimizationDbContext).Assembly);
    }
}
