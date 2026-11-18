using FleetOpt.Modules.Fleet;
using FleetOpt.Modules.Inventory;
using FleetOpt.Api.ErrorHandling;
using System.Text.Json.Serialization; 
using FleetOpt.Modules.Deliveries;
using FleetOpt.Modules.Optimization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddFleetModule(builder.Configuration); // Add the Fleet module services to the DI container
builder.Services.AddInventoryModule(builder.Configuration); // Add the Inventory module services to the DI container
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSingleton(TimeProvider.System);   // the real clock, shared by all modules
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddDeliveriesModule(builder.Configuration);
builder.Services.AddOptimizationModule(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapFleetEndpoints(); // Map the Fleet module endpoints
app.MapInventoryEndpoints(); // Map the Inventory module endpoints
app.MapDeliveriesEndpoints(); // Map the Deliveries module endpoints
app.MapOptimizationEndpoints(); // Map the Optimization module endpoints




app.Run();


