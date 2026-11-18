using FleetOpt.Modules.Inventory.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using FleetOpt.Modules.Inventory.Domain;
using FleetOpt.Modules.Inventory.Features.CreateWarehouse;
using FleetOpt.Modules.Inventory.Features.CreateProduct;
using FleetOpt.Modules.Inventory.Features.ReceiveStock;
using FleetOpt.Modules.Inventory.Features.GetWarehouseStock;
using FleetOpt.Modules.Inventory.Contracts;

namespace FleetOpt.Modules.Inventory;

public static class InventoryModule
{
    private const string ConnectionStringName = "FleetOpt";

    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' not found.");

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "inventory")));

        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<CreateWarehouseHandler>();
        services.AddScoped<CreateProductHandler>();
        services.AddScoped<ReceiveStockHandler>();
        services.AddScoped<GetWarehouseStockHandler>();
        services.AddScoped<IInventoryModuleApi, InventoryModuleApi>();



        return services;
    }

    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory").WithTags("Inventory");
        group.MapCreateWarehouse();
        group.MapCreateProduct();
        group.MapReceiveStock();
        group.MapGetWarehouseStock();

        return app;
    }
}
