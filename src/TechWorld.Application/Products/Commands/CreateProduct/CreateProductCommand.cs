using MediatR;
using TechWorld.Application.Products.Queries.GetProductById;

namespace TechWorld.Application.Products.Commands.CreateProduct;

public record SpecificationInput(string Label, string Value, int DisplayOrder = 0);

public record CreateProductCommand(
    string Name,
    decimal Price,
    string Category,
    string? Description,
    string? ImageUrl,
    int Stock,
    int DiscountPercentage,
    IEnumerable<SpecificationInput> Specifications
) : IRequest<ProductDetailDto>;
