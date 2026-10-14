using AulaPedidos.Application;
using AulaPedidos.Api.Endpoints;
using AulaPedidos.Infrastructure;
using AulaPedidos.Infrastructure.Persistence;
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
await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<AulaPedidosDbContext>().Database.EnsureCreatedAsync();
}
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.MapHealthChecks("/health");
app.MapProductEndpoints();
app.Run();
public partial class Program;
