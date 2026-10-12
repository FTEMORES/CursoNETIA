using AulaPedidos.Application.Abstractions.Persistence;

namespace AulaPedidos.Infrastructure.Persistence;

public sealed class EfUnitOfWork(AulaPedidosDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}