using System.Linq.Expressions;
using AulaPedidos.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AulaPedidos.Infrastructure.Persistence;

public sealed class EfRepository<T>(AulaPedidosDbContext context) : IRepository<T> where T : class
{
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await context.Set<T>().AddAsync(entity, cancellationToken);
    }

    public Task RemoveAsync(T entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        cancellationToken.ThrowIfCancellationRequested();

        context.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task<List<T>> GetAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default,
        params string[] includeStrings) =>
        BuildQuery(predicate, orderBy, includeStrings).ToListAsync(cancellationToken);

    public Task<List<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default,
        params string[] includeStrings)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), pageNumber, "Page number must be at least one.");
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be at least one.");
        }

        return BuildQuery(predicate, orderBy, includeStrings)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<T> BuildQuery(
        Expression<Func<T, bool>>? predicate,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy,
        string[]? includeStrings)
    {
        IQueryable<T> query = context.Set<T>();

        foreach (var includeString in includeStrings ?? [])
        {
            if (!string.IsNullOrWhiteSpace(includeString))
            {
                query = query.Include(includeString);
            }
        }

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return orderBy?.Invoke(query) ?? query;
    }
}