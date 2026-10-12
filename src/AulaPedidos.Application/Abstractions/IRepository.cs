using AulaPedidos.Domain.Abstractions;

namespace AulaPedidos.Application.Abstractions;

/// <summary>
/// Contrato de acceso a raíces de agregado. Sin implementación en esta base.
/// </summary>
public interface IRepository<T> where T : class, IAggregateRoot
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    void Add(T entity);
}
