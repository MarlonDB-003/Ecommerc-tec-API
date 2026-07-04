using AutoMapper;
using MediatR;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Application.Products.Queries.GetProductById;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IMapper mapper) : IRequestHandler<UpdateProductCommand, ProductDetailDto>
{
    public async Task<ProductDetailDto> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAdmin)
            throw new ForbiddenException("Somente administradores podem editar produtos.");

        var product = await productRepository.GetByIdWithSpecificationsAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        product.Update(request.Name, request.Price, request.Category,
            request.Description, request.ImageUrl, request.Stock, request.DiscountPercentage, request.Brand);

        var oldSpecs = product.Specifications.ToList();
        var newSpecs = (request.Specifications ?? []).Select((s, i) =>
            ProductSpecification.Create(product.Id, s.Label, s.Value, s.DisplayOrder > 0 ? s.DisplayOrder : i)).ToList();

        await productRepository.ReplaceSpecificationsAsync(oldSpecs, newSpecs, ct);
        product.SetSpecifications(newSpecs);

        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<ProductDetailDto>(product);
    }
}
