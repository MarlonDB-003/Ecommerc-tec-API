using MediatR;
using TechWorld.Application.Products.Commands.CreateProduct;
using TechWorld.Application.Products.Queries.GetProductById;

namespace TechWorld.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    decimal Price,
    string Category,
    string? Description,
    string? ImageUrl,
    int Stock,
    int DiscountPercentage,
    IEnumerable<SpecificationInput> Specifications
) : IRequest<ProductDetailDto>;
