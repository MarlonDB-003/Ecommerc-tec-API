using AutoMapper;
using FluentAssertions;
using Moq;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Application.Products.Commands.CreateProduct;
using TechWorld.Application.Products.Queries.GetProductById;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.UnitTests.Application.Products;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _handler = new CreateProductCommandHandler(
            _productRepo.Object, _unitOfWork.Object, _currentUser.Object, _mapper.Object);
    }

    private static CreateProductCommand ValidCommand() => new(
        "RTX 4090", 7999m, "Componentes",
        "Placa de vídeo top", null, 5, 0, [], "NVIDIA");

    private static ProductDetailDto FakeDto() => new(
        Guid.NewGuid(), "RTX 4090", null, 7999m, 7999m,
        null, "Componentes", "NVIDIA", 5, true, 0, DateTime.UtcNow, []);

    [Fact]
    public async Task Handle_WhenUserIsNotAdmin_ThrowsForbiddenException()
    {
        _currentUser.Setup(u => u.IsAdmin).Returns(false);

        var act = async () => await _handler.Handle(ValidCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_CreatesProduct()
    {
        _currentUser.Setup(u => u.IsAdmin).Returns(true);
        _mapper.Setup(m => m.Map<ProductDetailDto>(It.IsAny<Product>())).Returns(FakeDto());

        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        _productRepo.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenUserIsAdmin_ReturnsProductDetailDto()
    {
        _currentUser.Setup(u => u.IsAdmin).Returns(true);
        var expected = FakeDto();
        _mapper.Setup(m => m.Map<ProductDetailDto>(It.IsAny<Product>())).Returns(expected);

        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_WithSpecifications_SetsSpecificationsOnProduct()
    {
        _currentUser.Setup(u => u.IsAdmin).Returns(true);
        _mapper.Setup(m => m.Map<ProductDetailDto>(It.IsAny<Product>())).Returns(FakeDto());

        var cmd = ValidCommand() with
        {
            Specifications = [
                new SpecificationInput("VRAM", "24GB"),
                new SpecificationInput("Interface", "PCIe 4.0"),
            ]
        };

        Product? capturedProduct = null;
        _productRepo
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => capturedProduct = p)
            .Returns(Task.CompletedTask);

        await _handler.Handle(cmd, CancellationToken.None);

        capturedProduct.Should().NotBeNull();
        capturedProduct!.Specifications.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_NonAdmin_DoesNotCallRepository()
    {
        _currentUser.Setup(u => u.IsAdmin).Returns(false);

        var act = async () => await _handler.Handle(ValidCommand(), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();

        _productRepo.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
