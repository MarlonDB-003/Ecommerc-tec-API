using AutoMapper;
using FluentAssertions;
using Moq;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Application.Orders.Commands.CreateOrder;
using TechWorld.Application.Orders.Queries.GetOrderById;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Enums;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.UnitTests.Application.Orders;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _handler = new GetOrderByIdQueryHandler(_orderRepo.Object, _currentUser.Object, _mapper.Object);
    }

    private static Order CreateOrder(Guid userId) =>
        Order.Create(userId, PaymentMethod.Pix, 1,
            "João", "joao@ex.com", "11999999999",
            "01310-100", "Av. Paulista", "100", null,
            "Bela Vista", "São Paulo", "SP");

    private static OrderDto FakeDto(Guid orderId) => new(
        orderId, Guid.NewGuid(), 0m, "Pending", "Pix", 1,
        "João", "joao@ex.com", "11999999999",
        "01310-100", "Av. Paulista", "100", null, "Bela Vista", "São Paulo", "SP",
        [], DateTime.UtcNow);

    [Fact]
    public async Task Handle_WhenOrderNotFound_ThrowsNotFoundException()
    {
        _orderRepo
            .Setup(r => r.GetByIdWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var act = async () => await _handler.Handle(new GetOrderByIdQuery(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenAdminAccessesAnyOrder_ReturnsDto()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId);

        _currentUser.Setup(u => u.IsAdmin).Returns(true);
        _currentUser.Setup(u => u.UserId).Returns(Guid.NewGuid()); // different user
        _orderRepo
            .Setup(r => r.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _mapper.Setup(m => m.Map<OrderDto>(order)).Returns(FakeDto(order.Id));

        var result = await _handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenOwnerAccessesOwnOrder_ReturnsDto()
    {
        var userId = Guid.NewGuid();
        var order = CreateOrder(userId);

        _currentUser.Setup(u => u.IsAdmin).Returns(false);
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _orderRepo
            .Setup(r => r.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _mapper.Setup(m => m.Map<OrderDto>(order)).Returns(FakeDto(order.Id));

        var result = await _handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenNonOwnerNonAdminAccessesOrder_ThrowsForbiddenException()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var order = CreateOrder(ownerId);

        _currentUser.Setup(u => u.IsAdmin).Returns(false);
        _currentUser.Setup(u => u.UserId).Returns(requesterId);
        _orderRepo
            .Setup(r => r.GetByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var act = async () => await _handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
