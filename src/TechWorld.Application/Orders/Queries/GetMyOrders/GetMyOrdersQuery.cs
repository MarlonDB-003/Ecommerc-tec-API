using MediatR;
using TechWorld.Application.Common;
using TechWorld.Application.Orders.Commands.CreateOrder;

namespace TechWorld.Application.Orders.Queries.GetMyOrders;

public record GetMyOrdersQuery(int Page = 1, int PageSize = 10) : IRequest<PagedList<OrderDto>>;
