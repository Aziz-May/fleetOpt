using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FleetOpt.Modules.Fleet.Persistence;

namespace FleetOpt.Modules.Fleet;
public static class FleetModule
{
    public static IServiceCollection AddFleetModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FleetOpt") ?? throw new InvalidOperationException("Connection string 'FleetDb' not found."); 
        services.AddDbContext<FleetDbContext>(options => 
        options.UseSqlServer(connectionString , sql =>
        sql.MigrationsHistoryTable("__EFMigrationsHistory", "fleet") //specify the schema for the migrations history table
        ));
        return services ;
    }
    
}