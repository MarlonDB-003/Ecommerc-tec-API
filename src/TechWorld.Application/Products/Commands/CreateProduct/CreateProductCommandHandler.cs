using AutoMapper;
using MediatR;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Application.Products.Queries.GetProductById;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IMapper mapper) : IRequestHandler<CreateProductCommand, ProductDetailDto>
{
    public async Task<ProductDetailDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAdmin)
            throw new ForbiddenException("Somente administradores podem criar produtos.");

        var product = Product.Create(
            request.Name, request.Price, request.Category,
            request.Description, request.ImageUrl,
            request.Stock, request.DiscountPercentage, request.Brand);

        var specs = (request.Specifications ?? []).Select((s, i) =>
            ProductSpecification.Create(product.Id, s.Label, s.Value, s.DisplayOrder > 0 ? s.DisplayOrder : i)).ToList();

        product.SetSpecifications(specs);

        await productRepository.AddAsync(product, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<ProductDetailDto>(product);
    }
}
