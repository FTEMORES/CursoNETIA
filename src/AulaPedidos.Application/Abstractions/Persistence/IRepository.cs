using System.Linq.Expressions;

namespace AulaPedidos.Application.Abstractions.Persistence;

public interface IRepository<T> where T : class
{
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    Task RemoveAsync(T entity, CancellationToken cancellationToken = default);

    Task<List<T>> GetAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default,
        params string[] includeStrings);

    Task<List<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default,
        params string[] includeStrings);
}
