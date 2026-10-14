using AulaPedidos.Application.Products;
using Mediator;

namespace AulaPedidos.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var products = endpoints.MapGroup("/products");
        products.MapPost("", async (ProductRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var product = await sender.Send(new CreateProductCommand(request.Price, request.Description, request.AlternateDescription), cancellationToken);
            return Results.Created($"/products/{product.Id}", product);
        });
        products.MapGet("", async (ISender sender, CancellationToken cancellationToken) => Results.Ok(await sender.Send(new GetProductsQuery(), cancellationToken)));
        products.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var product = await sender.Send(new GetProductByIdQuery(id), cancellationToken);
            return product is null ? Results.NotFound() : Results.Ok(product);
        });
        products.MapPut("/{id:guid}", async (Guid id, ProductRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var product = await sender.Send(new UpdateProductCommand(id, request.Price, request.Description, request.AlternateDescription), cancellationToken);
            return product is null ? Results.NotFound() : Results.Ok(product);
        });
        products.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            await sender.Send(new DeleteProductCommand(id), cancellationToken) ? Results.NoContent() : Results.NotFound());
        return endpoints;
    }

    public sealed record ProductRequest(decimal Price, string? Description, string? AlternateDescription);
}
