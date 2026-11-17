using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FleetOpt.Modules.Fleet.Persistence;
using FleetOpt.Modules.Fleet.Domain;
using FleetOpt.Modules.Fleet.Features.RegisterTruck;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;


namespace FleetOpt.Modules.Fleet;

public static class FleetModule
{
    public static IServiceCollection AddFleetModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FleetOpt") ?? throw new InvalidOperationException("Connection string 'FleetDb' not found.");
        services.AddDbContext<FleetDbContext>(options =>
        options.UseSqlServer(connectionString, sql =>
        sql.MigrationsHistoryTable("__EFMigrationsHistory", "fleet") //specify the schema for the migrations history table
        ));
        services.AddScoped<ITruckRepository, TruckRepository>();
        services.AddScoped<RegisterTruckHandler>();
        return services;
    }
    public static IEndpointRouteBuilder MapFleetEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/api/fleet").WithTags("Fleet");
    group.MapRegisterTruck();
    return app;
}


}