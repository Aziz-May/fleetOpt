using FleetOpt.Modules.Deliveries.Domain;
using FleetOpt.Modules.Deliveries.Features.CreateDelivery;
using FleetOpt.Modules.Deliveries.Features.GetDeliveryById;
using FleetOpt.Modules.Deliveries.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FleetOpt.Modules.Deliveries;

public static class DeliveriesModule
{
    private const string ConnectionStringName = "FleetOpt";

    public static IServiceCollection AddDeliveriesModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' not found.");

        services.AddDbContext<DeliveriesDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "deliveries")));

        services.AddScoped<IDeliveryRepository, DeliveryRepository>();
        services.AddScoped<CreateDeliveryHandler>();
        services.AddScoped<GetDeliveryByIdHandler>();
        return services;
    }

    public static IEndpointRouteBuilder MapDeliveriesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/deliveries").WithTags("Deliveries");
        group.MapCreateDelivery();
        group.MapGetDeliveryById();
        return app;
    }
}
