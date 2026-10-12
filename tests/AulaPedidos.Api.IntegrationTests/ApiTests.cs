using System.Net;
using System.Text.Json;
using AulaPedidos.Application.Abstractions.Persistence;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace AulaPedidos.Api.IntegrationTests;

public sealed class ApiTests
{
    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    [InlineData("Staging")]
    public async Task HealthUsesAspNetCoreHealthChecksInEveryEnvironment(string environment)
    {
        await using var factory = CreateFactory(environment);
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/health", CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(CancellationToken.None));
        var report = await factory.Services.GetRequiredService<HealthCheckService>()
            .CheckHealthAsync(CancellationToken.None);
        Assert.Equal(HealthStatus.Healthy, report.Status);
        Assert.Empty(report.Entries);
    }

    [Theory]
    [InlineData("Development", "/health/live")]
    [InlineData("Production", "/health/live")]
    [InlineData("Development", "/weatherforecast")]
    [InlineData("Production", "/weatherforecast")]
    public async Task RemovedAndTemplateRoutesReturnNotFound(string environment, string path)
    {
        await using var factory = CreateFactory(environment);
        using var client = factory.CreateClient();
        using var response = await client.GetAsync(path, CancellationToken.None);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OpenApiIsAValidDocumentInDevelopment()
    {
        await using var factory = CreateFactory("Development");
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/openapi/v1.json", CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(CancellationToken.None));
        Assert.StartsWith("3.", document.RootElement.GetProperty("openapi").GetString());
        Assert.True(document.RootElement.TryGetProperty("paths", out _));
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public async Task OpenApiIsNotPublishedOutsideDevelopment(string environment)
    {
        await using var factory = CreateFactory(environment);
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/openapi/v1.json", CancellationToken.None);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ApiResolvesTheOfficialScopedMediatorAndPersistenceContractsWithoutBusinessEndpoints()
    {
        await using var factory = CreateFactory("Production");
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        using var secondScope = factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        Assert.Equal(typeof(Program).Assembly, mediator.GetType().Assembly);
        Assert.Equal("Mediator", mediator.GetType().Namespace);
        Assert.Same(mediator, scope.ServiceProvider.GetRequiredService<IMediator>());
        Assert.Same(mediator, scope.ServiceProvider.GetRequiredService<ISender>());
        Assert.Same(mediator, scope.ServiceProvider.GetRequiredService<IPublisher>());
        Assert.NotSame(mediator, secondScope.ServiceProvider.GetRequiredService<IMediator>());
        Assert.NotNull(scope.ServiceProvider.GetService<IRepository<RepositoryProbe>>());
        Assert.Single(scope.ServiceProvider.GetServices<IRepository<RepositoryProbe>>());
        Assert.NotNull(scope.ServiceProvider.GetService<IUnitOfWork>());
        var endpoints = factory.Services.GetRequiredService<EndpointDataSource>().Endpoints;
        var endpoint = Assert.Single(endpoints.OfType<RouteEndpoint>());
        Assert.Equal("/health", endpoint.RoutePattern.RawText);
        Assert.Null(endpoint.Metadata.GetMetadata<IAuthorizeData>());
    }

    private static WebApplicationFactory<Program> CreateFactory(string environment) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.UseDefaultServiceProvider(options =>
            {
                options.ValidateScopes = true;
                options.ValidateOnBuild = true;
            });
        });

    private sealed class RepositoryProbe;
}