using AutoMapper;
using MediatR;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Application.Orders.Commands.CreateOrder;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Application.Orders.Queries.GetMyOrders;

public class GetMyOrdersQueryHandler(
    IOrderRepository orderRepository,
    ICurrentUserService currentUser,
    IMapper mapper) : IRequestHandler<GetMyOrdersQuery, IEnumerable<OrderDto>>
{
    public async Task<IEnumerable<OrderDto>> Handle(GetMyOrdersQuery request, CancellationToken ct)
    {
        if (currentUser.UserId is null)
            throw new ForbiddenException();

        var orders = await orderRepository.GetByUserIdAsync(currentUser.UserId.Value, ct);

        return mapper.Map<IEnumerable<OrderDto>>(orders);
    }
}
