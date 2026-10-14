using AulaPedidos.Domain.Abstractions;

namespace AulaPedidos.Domain.Products;

public sealed class Product : IAggregateRoot
{
    public Product(Guid id, decimal price, string? description, string? alternateDescription)
    {
        Id = id;
        Price = price;
        Description = description;
        AlternateDescription = alternateDescription;
    }

    public Guid Id { get; private set; }
    public decimal Price { get; private set; }
    public string? Description { get; private set; }
    public string? AlternateDescription { get; private set; }

    public void Update(decimal price, string? description, string? alternateDescription)
    {
        Price = price;
        Description = description;
        AlternateDescription = alternateDescription;
    }
}
