using AutoMapper;
using FluentAssertions;
using Moq;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Products.Queries.GetProductById;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.UnitTests.Application.Products;

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _handler = new GetProductByIdQueryHandler(_productRepo.Object, _mapper.Object);
    }

    private static ProductDetailDto FakeDto(Guid id) => new(
        id, "RTX 4090", null, 7999m, 7999m,
        null, "Componentes", "NVIDIA", 10, true, 0, DateTime.UtcNow, []);

    [Fact]
    public async Task Handle_WhenProductNotFound_ThrowsNotFoundException()
    {
        _productRepo
            .Setup(r => r.GetByIdWithSpecificationsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var act = async () => await _handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProductFound_ReturnsMappedDto()
    {
        var product = Product.Create("RTX 4090", 7999m, "Componentes", stock: 10);
        var expected = FakeDto(product.Id);

        _productRepo
            .Setup(r => r.GetByIdWithSpecificationsAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapper.Setup(m => m.Map<ProductDetailDto>(product)).Returns(expected);

        var result = await _handler.Handle(new GetProductByIdQuery(product.Id), CancellationToken.None);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_WhenProductFound_LoadsWithSpecifications()
    {
        var product = Product.Create("RTX 4090", 7999m, "Componentes");
        _productRepo
            .Setup(r => r.GetByIdWithSpecificationsAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapper.Setup(m => m.Map<ProductDetailDto>(product)).Returns(FakeDto(product.Id));

        await _handler.Handle(new GetProductByIdQuery(product.Id), CancellationToken.None);

        _productRepo.Verify(r => r.GetByIdWithSpecificationsAsync(product.Id, It.IsAny<CancellationToken>()), Times.Once);
        _productRepo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
