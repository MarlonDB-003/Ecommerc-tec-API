using AutoMapper;
using MediatR;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IMapper mapper) : IRequestHandler<CreateOrderCommand, OrderDto>
{
    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            throw new ForbiddenException("Autentique-se para realizar um pedido.");

        var order = Order.Create(
            currentUser.UserId.Value,
            request.PaymentMethod,
            request.Installments,
            request.CustomerInfo.Name,
            request.CustomerInfo.Email,
            request.CustomerInfo.Phone,
            request.AddressInfo.Cep,
            request.AddressInfo.Street,
            request.AddressInfo.Number,
            request.AddressInfo.Complement,
            request.AddressInfo.Neighborhood,
            request.AddressInfo.City,
            request.AddressInfo.State);

        foreach (var item in request.Items)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId, ct)
                ?? throw new NotFoundException(nameof(Product), item.ProductId);

            if (!product.IsActive)
                throw new InvalidOperationException($"Produto '{product.Name}' não está disponível.");

            if (product.Stock < item.Quantity)
                throw new InvalidOperationException($"Estoque insuficiente para '{product.Name}'.");

            order.AddItem(product.Id, product.Name, item.Quantity, product.CalculateDiscountedPrice());
            product.UpdateStock(-item.Quantity);
            productRepository.Update(product);
        }

        await orderRepository.AddAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return mapper.Map<OrderDto>(order);
    }
}
