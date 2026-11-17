using FleetOpt.Modules.Fleet;
using FleetOpt.Api.ErrorHandling;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddFleetModule(builder.Configuration); // Add the Fleet module services to the DI container
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapFleetEndpoints(); // Map the Fleet module endpoints




app.Run();


