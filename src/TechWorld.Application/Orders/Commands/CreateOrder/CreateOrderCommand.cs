using MediatR;
using TechWorld.Domain.Enums;

namespace TechWorld.Application.Orders.Commands.CreateOrder;

public record OrderItemInput(Guid ProductId, int Quantity);

public record CustomerInfoInput(string Name, string Email, string Phone);

public record AddressInfoInput(
    string Cep,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State);

public record CreateOrderCommand(
    IEnumerable<OrderItemInput> Items,
    CustomerInfoInput CustomerInfo,
    AddressInfoInput AddressInfo,
    PaymentMethod PaymentMethod,
    int Installments = 1
) : IRequest<OrderDto>;
