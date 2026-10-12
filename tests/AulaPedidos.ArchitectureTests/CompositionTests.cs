using AulaPedidos.Application;
using AulaPedidos.Application.Abstractions.Persistence;
using AulaPedidos.Infrastructure;
using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AulaPedidos.ArchitectureTests;

public sealed class CompositionTests
{
    [Fact]
    public void ApplicationRejectsNullServices()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            AulaPedidos.Application.DependencyInjection.AddApplication(null!));
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void InfrastructureRejectsNullServices()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            AulaPedidos.Infrastructure.DependencyInjection.AddInfrastructure(null!, new ConfigurationBuilder().Build()));
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void InfrastructureRejectsNullConfiguration()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new ServiceCollection().AddInfrastructure(null!));
        Assert.Equal("configuration", exception.ParamName);
    }

    [Fact]
    public void CompositionRegistersPersistenceContractsWithConfiguredStorage()
    {
        var services = new ServiceCollection();
        Assert.Same(services, services.AddApplication());
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:"
            })
            .Build();

        Assert.Same(services, services.AddInfrastructure(configuration));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IRepository<>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IUnitOfWork));
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
        using var scope = provider.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetService<IRepository<RepositoryProbe>>());
        Assert.NotNull(scope.ServiceProvider.GetService<IUnitOfWork>());
    }

    [Fact]
    public void ApplicationRegistrationIsIdempotent()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        var original = services.ToArray();
        services.AddApplication();
        Assert.Equal(original, services.ToArray());
    }

    private sealed class RepositoryProbe;
}