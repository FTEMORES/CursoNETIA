using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AulaPedidos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ISender>(provider => provider.GetRequiredService<IMediator>());
        services.TryAddScoped<IPublisher>(provider => provider.GetRequiredService<IMediator>());

        return services;
    }
}
