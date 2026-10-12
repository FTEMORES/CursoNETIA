using AulaPedidos.Application;
using AulaPedidos.Infrastructure;
using Mediator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMediator(options =>
{
    options.Assemblies = [typeof(AulaPedidos.Application.DependencyInjection)];
    options.ServiceLifetime = ServiceLifetime.Scoped;
});
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");
app.Run();

public partial class Program;

