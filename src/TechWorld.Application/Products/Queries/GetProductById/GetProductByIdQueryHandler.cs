using AutoMapper;
using MediatR;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Application.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler(IProductRepository productRepository, IMapper mapper)
    : IRequestHandler<GetProductByIdQuery, ProductDetailDto>
{
    public async Task<ProductDetailDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await productRepository.GetByIdWithSpecificationsAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        return mapper.Map<ProductDetailDto>(product);
    }
}
