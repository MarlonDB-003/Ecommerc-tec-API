using MediatR;
using TechWorld.Application.Common;

namespace TechWorld.Application.Products.Queries.GetProducts;

public record GetProductsQuery(
    string? Category,
    string? Search,
    bool? IsActive,
    string SortBy = "createdat",
    bool Ascending = false,
    int Page = 1,
    int PageSize = 12
) : IRequest<PagedList<ProductListDto>>;
