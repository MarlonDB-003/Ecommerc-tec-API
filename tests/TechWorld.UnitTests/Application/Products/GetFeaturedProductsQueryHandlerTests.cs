using AutoMapper;
using FluentAssertions;
using Moq;
using TechWorld.Application.Products.Queries.GetFeaturedProducts;
using TechWorld.Application.Products.Queries.GetProducts;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.UnitTests.Application.Products;

public class GetFeaturedProductsQueryHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly GetFeaturedProductsQueryHandler _handler;

    public GetFeaturedProductsQueryHandlerTests()
    {
        _handler = new GetFeaturedProductsQueryHandler(_productRepo.Object, _mapper.Object);
        _mapper
            .Setup(m => m.Map<IEnumerable<ProductListDto>>(It.IsAny<IEnumerable<Product>>()))
            .Returns([]);
    }

    [Fact]
    public async Task Handle_ReturnsMappedProducts()
    {
        _productRepo
            .Setup(r => r.GetFeaturedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new GetFeaturedProductsQuery("discounts", 10), CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-10, 1)]
    [InlineData(51, 50)]
    [InlineData(100, 50)]
    public async Task Handle_ClampsLimitToValidRange(int rawLimit, int expectedClamped)
    {
        int capturedLimit = 0;
        _productRepo
            .Setup(r => r.GetFeaturedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<string, int, CancellationToken>((_, limit, _) => capturedLimit = limit)
            .ReturnsAsync([]);

        await _handler.Handle(new GetFeaturedProductsQuery("discounts", rawLimit), CancellationToken.None);

        capturedLimit.Should().Be(expectedClamped);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(10, 10)]
    [InlineData(50, 50)]
    public async Task Handle_WithValidLimit_PassesLimitToRepository(int limit, int expected)
    {
        int capturedLimit = 0;
        _productRepo
            .Setup(r => r.GetFeaturedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<string, int, CancellationToken>((_, l, _) => capturedLimit = l)
            .ReturnsAsync([]);

        await _handler.Handle(new GetFeaturedProductsQuery("discounts", limit), CancellationToken.None);

        capturedLimit.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_ForwardsTypeToRepository()
    {
        string capturedType = "";
        _productRepo
            .Setup(r => r.GetFeaturedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<string, int, CancellationToken>((type, _, _) => capturedType = type)
            .ReturnsAsync([]);

        await _handler.Handle(new GetFeaturedProductsQuery("gaming", 5), CancellationToken.None);

        capturedType.Should().Be("gaming");
    }
}
