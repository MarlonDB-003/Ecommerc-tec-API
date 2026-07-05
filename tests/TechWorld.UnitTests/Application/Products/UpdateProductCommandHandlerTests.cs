using AutoMapper;
using FluentAssertions;
using Moq;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Application.Products.Commands.CreateProduct;
using TechWorld.Application.Products.Commands.UpdateProduct;
using TechWorld.Application.Products.Queries.GetProductById;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.UnitTests.Application.Products;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _handler = new UpdateProductCommandHandler(
            _productRepo.Object, _unitOfWork.Object, _currentUser.Object, _mapper.Object);
    }

    private static UpdateProductCommand ValidCommand(Guid id) => new(
        id, "Monitor 4K Atualizado", 2200m, "Computadores",
        null, null, 3, 5, [], "Dell");

    private static Product ExistingProduct() =>
        Product.Create("Monitor 4K", 2000m, "Computadores", stock: 5);

    private static ProductDetailDto FakeDto() => new(
        Guid.NewGuid(), "Monitor 4K Atualizado", null, 2200m, 2090m,
        null, "Computadores", "Dell", 3, true, 5, DateTime.UtcNow, []);

    [Fact]
    public async Task Handle_WhenNotAdmin_ThrowsForbiddenException()
    {
        _currentUser.Setup(u => u.IsAdmin).Returns(false);
        var act = async () => await _handler.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ThrowsNotFoundException()
    {
        _currentUser.Setup(u => u.IsAdmin).Returns(true);
        _productRepo
            .Setup(r => r.GetByIdWithSpecificationsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var act = async () => await _handler.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenValid_UpdatesProductAndSaves()
    {
        var product = ExistingProduct();
        _currentUser.Setup(u => u.IsAdmin).Returns(true);
        _productRepo
            .Setup(r => r.GetByIdWithSpecificationsAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _productRepo
            .Setup(r => r.ReplaceSpecificationsAsync(
                It.IsAny<IEnumerable<ProductSpecification>>(),
                It.IsAny<IEnumerable<ProductSpecification>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mapper.Setup(m => m.Map<ProductDetailDto>(It.IsAny<Product>())).Returns(FakeDto());

        var result = await _handler.Handle(ValidCommand(product.Id), CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        result.Should().NotBeNull();
        result.Name.Should().Be("Monitor 4K Atualizado");
    }

    [Fact]
    public async Task Handle_WhenValid_ReplacesSpecifications()
    {
        var product = ExistingProduct();
        _currentUser.Setup(u => u.IsAdmin).Returns(true);
        _productRepo
            .Setup(r => r.GetByIdWithSpecificationsAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _productRepo
            .Setup(r => r.ReplaceSpecificationsAsync(
                It.IsAny<IEnumerable<ProductSpecification>>(),
                It.IsAny<IEnumerable<ProductSpecification>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mapper.Setup(m => m.Map<ProductDetailDto>(It.IsAny<Product>())).Returns(FakeDto());

        var cmd = ValidCommand(product.Id) with
        {
            Specifications = [new SpecificationInput("Resolução", "3840x2160")]
        };

        await _handler.Handle(cmd, CancellationToken.None);

        _productRepo.Verify(r => r.ReplaceSpecificationsAsync(
            It.IsAny<IEnumerable<ProductSpecification>>(),
            It.Is<IEnumerable<ProductSpecification>>(s => s.Count() == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonAdmin_DoesNotCallRepository()
    {
        _currentUser.Setup(u => u.IsAdmin).Returns(false);
        var act = async () => await _handler.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
        _productRepo.Verify(r => r.GetByIdWithSpecificationsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
