using AulaPedidos.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AulaPedidos.Infrastructure.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_devuelve_la_misma_coleccion()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var result = services.AddInfrastructure(configuration);

        Assert.Same(services, result);
    }

    [Fact]
    public void AddInfrastructure_no_registra_servicios()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddInfrastructure(configuration);

        Assert.Empty(services);
    }
}
