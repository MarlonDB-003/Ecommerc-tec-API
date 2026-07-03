using MediatR;
using TechWorld.Application.Products.Queries.GetProducts;

namespace TechWorld.Application.Products.Queries.GetFeaturedProducts;

public record GetFeaturedProductsQuery(string Type, int Limit = 10) : IRequest<IEnumerable<ProductListDto>>;
