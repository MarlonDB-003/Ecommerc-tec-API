using AutoMapper;
using MediatR;
using TechWorld.Application.Common;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Application.Orders.Commands.CreateOrder;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Application.Orders.Queries.GetMyOrders;

public class GetMyOrdersQueryHandler(
    IOrderRepository orderRepository,
    ICurrentUserService currentUser,
    IMapper mapper) : IRequestHandler<GetMyOrdersQuery, PagedList<OrderDto>>
{
    public async Task<PagedList<OrderDto>> Handle(GetMyOrdersQuery request, CancellationToken ct)
    {
        if (currentUser.UserId is null)
            throw new ForbiddenException();

        var (orders, totalCount) = await orderRepository.GetByUserIdPagedAsync(
            currentUser.UserId.Value, request.Page, request.PageSize, ct);

        var dtos = mapper.Map<IEnumerable<OrderDto>>(orders);
        return PagedList<OrderDto>.Create(dtos, totalCount, request.Page, request.PageSize);
    }
}
