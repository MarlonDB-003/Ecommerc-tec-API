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
        var products = await productRepository.GetFeaturedAsync(request.Type, request.Limit, ct);

        return mapper.Map<IEnumerable<ProductListDto>>(products);
    }
}
