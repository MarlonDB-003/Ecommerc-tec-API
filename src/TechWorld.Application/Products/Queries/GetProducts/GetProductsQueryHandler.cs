using AutoMapper;
using MediatR;
using TechWorld.Application.Common;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Application.Products.Queries.GetProducts;

public class GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
    : IRequestHandler<GetProductsQuery, PagedList<ProductListDto>>
{
    public async Task<PagedList<ProductListDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var (items, totalCount) = await productRepository.GetPagedAsync(
            request.Category,
            request.Search,
            request.IsActive,
            request.SortBy,
            request.Ascending,
            request.Page,
            request.PageSize,
            ct);

        var dtos = mapper.Map<IEnumerable<ProductListDto>>(items);

        return PagedList<ProductListDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }
}
