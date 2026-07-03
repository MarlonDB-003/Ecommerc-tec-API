using MediatR;

namespace TechWorld.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDetailDto>;
