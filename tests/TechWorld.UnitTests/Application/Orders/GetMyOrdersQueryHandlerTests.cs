using AutoMapper;
using FluentAssertions;
using Moq;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Application.Orders.Commands.CreateOrder;
using TechWorld.Application.Orders.Queries.GetMyOrders;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Enums;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.UnitTests.Application.Orders;

public class GetMyOrdersQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly GetMyOrdersQueryHandler _handler;

    public GetMyOrdersQueryHandlerTests()
    {
        _handler = new GetMyOrdersQueryHandler(_orderRepo.Object, _currentUser.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsForbiddenException()
    {
        _currentUser.Setup(u => u.UserId).Returns((Guid?)null);

        var act = async () => await _handler.Handle(new GetMyOrdersQuery(), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenAuthenticated_ReturnsPagedList()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _orderRepo
            .Setup(r => r.GetByUserIdPagedAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<Order>(), 0));
        _mapper
            .Setup(m => m.Map<IEnumerable<OrderDto>>(It.IsAny<IEnumerable<Order>>()))
            .Returns([]);

        var result = await _handler.Handle(new GetMyOrdersQuery(1, 10), CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectPaginationMetadata()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _orderRepo
            .Setup(r => r.GetByUserIdPagedAsync(userId, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<Order>(), 15));
        _mapper
            .Setup(m => m.Map<IEnumerable<OrderDto>>(It.IsAny<IEnumerable<Order>>()))
            .Returns([]);

        var result = await _handler.Handle(new GetMyOrdersQuery(2, 5), CancellationToken.None);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(3);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue(); // page 2 of 3
    }

    [Fact]
    public async Task Handle_PassesCorrectUserIdToRepository()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _orderRepo
            .Setup(r => r.GetByUserIdPagedAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<Order>(), 0));
        _mapper
            .Setup(m => m.Map<IEnumerable<OrderDto>>(It.IsAny<IEnumerable<Order>>()))
            .Returns([]);

        await _handler.Handle(new GetMyOrdersQuery(), CancellationToken.None);

        _orderRepo.Verify(r => r.GetByUserIdPagedAsync(userId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
