using AulaPedidos.Domain.Products;

namespace AulaPedidos.Application.Products;

public sealed record ProductDto(Guid Id, decimal Price, string? Description, string? AlternateDescription)
{
    public static ProductDto FromProduct(Product product) =>
        new(product.Id, product.Price, product.Description, product.AlternateDescription);
}
