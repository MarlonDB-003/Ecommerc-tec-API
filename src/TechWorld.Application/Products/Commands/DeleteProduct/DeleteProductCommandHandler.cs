using MediatR;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAdmin)
            throw new ForbiddenException("Somente administradores podem remover produtos.");

        var product = await productRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        productRepository.Remove(product);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
