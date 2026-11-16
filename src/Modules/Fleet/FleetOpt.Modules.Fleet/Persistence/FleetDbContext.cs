using Microsoft.EntityFrameworkCore;
using FleetOpt.Modules.Fleet.Domain;

namespace FleetOpt.Modules.Fleet.Persistence;

internal sealed class FleetDbContext : DbContext
{
    public DbSet<Truck> Trucks => Set<Truck>();
    
    public FleetDbContext(DbContextOptions<FleetDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("fleet");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FleetDbContext).Assembly);

       
    }
}