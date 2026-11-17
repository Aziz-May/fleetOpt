using FleetOpt.Modules.Fleet.Persistence;
using FleetOpt.Modules.Inventory.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace FleetOpt.Api.IntegrationTests;

public sealed class FleetApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")   // same image as dev
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");   // not Development →  user-secrets are NOT loaded
        builder.UseSetting("ConnectionStrings:FleetOpt", _sqlContainer.GetConnectionString());
    }

    // xUnit calls this ONCE before the tests of a class run
    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        using var scope = Services.CreateScope();          // first access to Services → app boots here
        var db = scope.ServiceProvider.GetRequiredService<FleetDbContext>();
        await db.Database.MigrateAsync();                  // applies YOUR migration to the empty DB
        var inventoryDb = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        await inventoryDb.Database.MigrateAsync();

    }

    // xUnit calls this ONCE after the tests
    async Task IAsyncLifetime.DisposeAsync()
    {
        await _sqlContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
