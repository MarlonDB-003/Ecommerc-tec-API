using AutoMapper;
using TechWorld.Application.Orders.Commands.CreateOrder;
using TechWorld.Domain.Entities;

namespace TechWorld.Application.Common.Mappings;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<OrderItem, OrderItemDto>()
            .ForCtorParam("Subtotal", opt => opt.MapFrom(i => i.Quantity * i.UnitPrice));

        CreateMap<Order, OrderDto>()
            .ForCtorParam("Status", opt => opt.MapFrom(o => o.Status.ToString()))
            .ForCtorParam("PaymentMethod", opt => opt.MapFrom(o => o.PaymentMethod.ToString()));
    }
}
