using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using TechWorld.Application.Common.Mappings;
using TechWorld.Application.Orders.Commands.CreateOrder;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Enums;

namespace TechWorld.UnitTests.Application.Mappings;

public class OrderMappingProfileTests
{
    private static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(OrderMappingProfile).Assembly));
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    private static Order CreateOrderWithItems()
    {
        var order = Order.Create(
            Guid.NewGuid(), PaymentMethod.CreditCard, 3,
            "Maria Silva", "maria@exemplo.com", "21988887777",
            "22041-001", "Rua das Flores", "200", "Apto 5",
            "Tijuca", "Rio de Janeiro", "RJ");

        order.AddItem(Guid.NewGuid(), "Notebook Dell", 1, 4500m);
        order.AddItem(Guid.NewGuid(), "Mouse Logitech", 2, 150m);

        return order;
    }

    [Fact]
    public void ConfigurationIsValid()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(OrderMappingProfile).Assembly));
        var mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_Order_To_OrderDto_MapsBaseFields()
    {
        var mapper = CreateMapper();
        var order = CreateOrderWithItems();

        var dto = mapper.Map<OrderDto>(order);

        dto.Id.Should().Be(order.Id);
        dto.UserId.Should().Be(order.UserId);
        dto.CustomerName.Should().Be("Maria Silva");
        dto.CustomerEmail.Should().Be("maria@exemplo.com");
        dto.CustomerPhone.Should().Be("21988887777");
        dto.AddressCep.Should().Be("22041-001");
        dto.AddressState.Should().Be("RJ");
        dto.Installments.Should().Be(3);
    }

    [Fact]
    public void Map_Order_To_OrderDto_ConvertsStatusToString()
    {
        var mapper = CreateMapper();
        var order = CreateOrderWithItems();

        var dto = mapper.Map<OrderDto>(order);

        dto.Status.Should().Be("Pending");
    }

    [Fact]
    public void Map_Order_To_OrderDto_ConvertsPaymentMethodToString()
    {
        var mapper = CreateMapper();
        var order = CreateOrderWithItems();

        var dto = mapper.Map<OrderDto>(order);

        dto.PaymentMethod.Should().Be("CreditCard");
    }

    [Fact]
    public void Map_Order_To_OrderDto_MapsItems()
    {
        var mapper = CreateMapper();
        var order = CreateOrderWithItems();

        var dto = mapper.Map<OrderDto>(order);

        dto.Items.Should().HaveCount(2);
    }

    [Fact]
    public void Map_Order_To_OrderDto_ComputesTotalAmount()
    {
        var mapper = CreateMapper();
        var order = CreateOrderWithItems();

        var dto = mapper.Map<OrderDto>(order);

        dto.TotalAmount.Should().Be(4800m); // 4500 + 2*150
    }

    [Fact]
    public void Map_OrderItem_To_OrderItemDto_ComputesSubtotal()
    {
        var mapper = CreateMapper();
        var order = CreateOrderWithItems();

        var dto = mapper.Map<OrderDto>(order);

        var notebookItem = dto.Items.First(i => i.ProductName == "Notebook Dell");
        notebookItem.Subtotal.Should().Be(4500m);

        var mouseItem = dto.Items.First(i => i.ProductName == "Mouse Logitech");
        mouseItem.Subtotal.Should().Be(300m);
    }
}
