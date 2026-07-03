using TechWorld.Domain.Enums;

namespace TechWorld.Application.Orders.Commands.CreateOrder;

public record OrderDto(
    Guid Id,
    Guid UserId,
    decimal TotalAmount,
    string Status,
    string PaymentMethod,
    int Installments,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    string AddressCep,
    string AddressStreet,
    string AddressNumber,
    string? AddressComplement,
    string AddressNeighborhood,
    string AddressCity,
    string AddressState,
    IEnumerable<OrderItemDto> Items,
    DateTime CreatedAt
);

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal
);
