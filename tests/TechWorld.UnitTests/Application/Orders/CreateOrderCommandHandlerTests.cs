using AutoMapper;
using FluentAssertions;
using Moq;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Application.Orders.Commands.CreateOrder;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Enums;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.UnitTests.Application.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _handler = new CreateOrderCommandHandler(
            _orderRepo.Object, _productRepo.Object, _unitOfWork.Object, _currentUser.Object, _mapper.Object);
    }

    private void SetupAuthenticatedUser(Guid? userId = null)
    {
        var id = userId ?? Guid.NewGuid();
        _currentUser.Setup(u => u.IsAuthenticated).Returns(true);
        _currentUser.Setup(u => u.UserId).Returns(id);
    }

    private static CreateOrderCommand ValidCommand(Guid productId) => new(
        Items: [new OrderItemInput(productId, 1)],
        CustomerInfo: new CustomerInfoInput("João Silva", "joao@ex.com", "11999999999"),
        AddressInfo: new AddressInfoInput("01310-100", "Av. Paulista", "100", null, "Bela Vista", "São Paulo", "SP"),
        PaymentMethod: PaymentMethod.Pix
    );

    private static OrderDto FakeOrderDto() => new(
        Guid.NewGuid(), Guid.NewGuid(), 999m, "Pending", "Pix", 1,
        "João", "joao@ex.com", "11999999999",
        "01310-100", "Av. Paulista", "100", null, "Bela Vista", "São Paulo", "SP",
        [], DateTime.UtcNow);

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ThrowsForbiddenException()
    {
        _currentUser.Setup(u => u.IsAuthenticated).Returns(false);
        _currentUser.Setup(u => u.UserId).Returns((Guid?)null);

        var act = async () => await _handler.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ThrowsForbiddenException()
    {
        _currentUser.Setup(u => u.IsAuthenticated).Returns(true);
        _currentUser.Setup(u => u.UserId).Returns((Guid?)null);

        var act = async () => await _handler.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ThrowsNotFoundException()
    {
        SetupAuthenticatedUser();
        var productId = Guid.NewGuid();
        _productRepo
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var act = async () => await _handler.Handle(ValidCommand(productId), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProductIsInactive_ThrowsInvalidOperationException()
    {
        SetupAuthenticatedUser();
        var product = Product.Create("Produto Inativo", 500m, "Gaming", stock: 10);
        product.Deactivate();

        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var act = async () => await _handler.Handle(ValidCommand(product.Id), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenStockIsInsufficient_ThrowsInvalidOperationException()
    {
        SetupAuthenticatedUser();
        var product = Product.Create("RTX 4090", 7999m, "Componentes", stock: 0);

        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var cmd = ValidCommand(product.Id) with
        {
            Items = [new OrderItemInput(product.Id, 5)]
        };

        var act = async () => await _handler.Handle(cmd, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenValid_DecreasesProductStock()
    {
        SetupAuthenticatedUser();
        var product = Product.Create("RTX 4090", 7999m, "Componentes", stock: 10);

        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapper.Setup(m => m.Map<OrderDto>(It.IsAny<Order>())).Returns(FakeOrderDto());

        var cmd = ValidCommand(product.Id) with
        {
            Items = [new OrderItemInput(product.Id, 3)]
        };

        await _handler.Handle(cmd, CancellationToken.None);

        product.Stock.Should().Be(7);
        _productRepo.Verify(r => r.Update(product), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValid_SavesOrderAndReturnsDto()
    {
        SetupAuthenticatedUser();
        var product = Product.Create("RTX 4090", 7999m, "Componentes", stock: 5);

        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapper.Setup(m => m.Map<OrderDto>(It.IsAny<Order>())).Returns(FakeOrderDto());

        var result = await _handler.Handle(ValidCommand(product.Id), CancellationToken.None);

        _orderRepo.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenValid_UsesDiscountedPrice()
    {
        SetupAuthenticatedUser();
        var product = Product.Create("Monitor", 1000m, "Computadores", stock: 10, discountPercentage: 20);

        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        Order? capturedOrder = null;
        _orderRepo
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((o, _) => capturedOrder = o)
            .Returns(Task.CompletedTask);
        _mapper.Setup(m => m.Map<OrderDto>(It.IsAny<Order>())).Returns(FakeOrderDto());

        await _handler.Handle(ValidCommand(product.Id), CancellationToken.None);

        capturedOrder.Should().NotBeNull();
        capturedOrder!.TotalAmount.Should().Be(800m); // 1000 * 0.80
    }
}
