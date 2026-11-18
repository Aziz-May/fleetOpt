using FleetOpt.Modules.Optimization.Domain;
using FleetOpt.Modules.Optimization.Features.GenerateDeliveryPlan;
using FleetOpt.Modules.Optimization.Features.GetDeliveryPlan;
using FleetOpt.Modules.Optimization.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FleetOpt.Modules.Optimization;

public static class OptimizationModule
{
    private const string ConnectionStringName = "FleetOpt";

    /// <remarks>
    /// Requires the Fleet, Inventory and Deliveries modules to be registered too: the handler needs
    /// their IxxxModuleApi contracts. The DI container fails fast at the first request if one is missing.
    /// </remarks>
    public static IServiceCollection AddOptimizationModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' not found.");

        services.AddDbContext<OptimizationDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "optimization")));

        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<GenerateDeliveryPlanHandler>();
        services.AddScoped<GetDeliveryPlanHandler>();
        return services;
    }

    public static IEndpointRouteBuilder MapOptimizationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/optimization").WithTags("Optimization");
        group.MapGenerateDeliveryPlan();
        group.MapGetDeliveryPlan();
        return app;
    }
}
