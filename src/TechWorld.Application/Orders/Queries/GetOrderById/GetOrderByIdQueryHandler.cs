using AutoMapper;
using MediatR;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Application.Orders.Commands.CreateOrder;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler(
    IOrderRepository orderRepository,
    ICurrentUserService currentUser,
    IMapper mapper) : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var order = await orderRepository.GetByIdWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Order), request.Id);

        if (!currentUser.IsAdmin && order.UserId != currentUser.UserId)
            throw new ForbiddenException();

        return mapper.Map<OrderDto>(order);
    }
}
