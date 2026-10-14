using AulaPedidos.Application.Abstractions.Persistence;
using AulaPedidos.Domain.Products;
using Mediator;

namespace AulaPedidos.Application.Products;

public sealed record CreateProductCommand(decimal Price, string? Description, string? AlternateDescription) : IRequest<ProductDto>;
public sealed record UpdateProductCommand(Guid Id, decimal Price, string? Description, string? AlternateDescription) : IRequest<ProductDto?>;
public sealed record DeleteProductCommand(Guid Id) : IRequest<bool>;
public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
public sealed record GetProductsQuery : IRequest<IReadOnlyList<ProductDto>>;

public sealed class CreateProductCommandHandler(IRepository<Product> repository, IUnitOfWork unitOfWork) : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async ValueTask<ProductDto> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product(Guid.NewGuid(), command.Price, command.Description, command.AlternateDescription);
        await repository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ProductDto.FromProduct(product);
    }
}

public sealed class UpdateProductCommandHandler(IRepository<Product> repository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateProductCommand, ProductDto?>
{
    public async ValueTask<ProductDto?> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = (await repository.GetAsync(candidate => candidate.Id == command.Id, cancellationToken: cancellationToken)).SingleOrDefault();
        if (product is null) return null;
        product.Update(command.Price, command.Description, command.AlternateDescription);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ProductDto.FromProduct(product);
    }
}

public sealed class DeleteProductCommandHandler(IRepository<Product> repository, IUnitOfWork unitOfWork) : IRequestHandler<DeleteProductCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = (await repository.GetAsync(candidate => candidate.Id == command.Id, cancellationToken: cancellationToken)).SingleOrDefault();
        if (product is null) return false;
        await repository.RemoveAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed class GetProductByIdQueryHandler(IRepository<Product> repository) : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public async ValueTask<ProductDto?> Handle(GetProductByIdQuery query, CancellationToken cancellationToken) =>
        (await repository.GetAsync(candidate => candidate.Id == query.Id, cancellationToken: cancellationToken)).Select(ProductDto.FromProduct).SingleOrDefault();
}

public sealed class GetProductsQueryHandler(IRepository<Product> repository) : IRequestHandler<GetProductsQuery, IReadOnlyList<ProductDto>>
{
    public async ValueTask<IReadOnlyList<ProductDto>> Handle(GetProductsQuery query, CancellationToken cancellationToken) =>
        (await repository.GetAsync(orderBy: products => products.OrderBy(product => product.Id), cancellationToken: cancellationToken)).Select(ProductDto.FromProduct).ToList();
}
