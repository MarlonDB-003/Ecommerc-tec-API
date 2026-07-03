using MediatR;
using TechWorld.Application.Orders.Commands.CreateOrder;

namespace TechWorld.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDto>;
