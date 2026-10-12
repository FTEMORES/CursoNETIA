namespace AulaPedidos.Domain.Abstractions;

/// <summary>
/// Marca las raíces de agregado que pueden gestionarse mediante repositorios.
/// </summary>
public interface IAggregateRoot
{
    Guid Id { get; }
}
