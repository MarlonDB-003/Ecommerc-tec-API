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
        var safePageSize = Math.Clamp(request.PageSize, 1, 100);

        var (items, totalCount) = await productRepository.GetPagedAsync(
            request.Category,
            request.Search,
            request.IsActive,
            request.Brand,
            request.SortBy,
            request.Ascending,
            request.Page,
            safePageSize,
            ct);

        var dtos = mapper.Map<IEnumerable<ProductListDto>>(items);

        return PagedList<ProductListDto>.Create(dtos, totalCount, request.Page, safePageSize);
    }
}
