using AutoMapper;
using MediatR;
using TechWorld.Application.Products.Queries.GetProducts;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Application.Products.Queries.GetFeaturedProducts;

public class GetFeaturedProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
    : IRequestHandler<GetFeaturedProductsQuery, IEnumerable<ProductListDto>>
{
    public async Task<IEnumerable<ProductListDto>> Handle(GetFeaturedProductsQuery request, CancellationToken ct)
    {
        var safeLimit = Math.Clamp(request.Limit, 1, 50);
        var products = await productRepository.GetFeaturedAsync(request.Type, safeLimit, ct);

        return mapper.Map<IEnumerable<ProductListDto>>(products);
    }
}
