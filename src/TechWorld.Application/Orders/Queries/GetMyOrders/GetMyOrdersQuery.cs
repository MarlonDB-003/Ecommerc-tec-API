using MediatR;
using TechWorld.Application.Orders.Commands.CreateOrder;

namespace TechWorld.Application.Orders.Queries.GetMyOrders;

public record GetMyOrdersQuery : IRequest<IEnumerable<OrderDto>>;
