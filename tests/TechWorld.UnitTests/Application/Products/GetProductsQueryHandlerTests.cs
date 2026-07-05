using AutoMapper;
using FluentAssertions;
using Moq;
using TechWorld.Application.Products.Queries.GetProducts;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.UnitTests.Application.Products;

public class GetProductsQueryHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly GetProductsQueryHandler _handler;

    public GetProductsQueryHandlerTests()
    {
        _handler = new GetProductsQueryHandler(_productRepo.Object, _mapper.Object);
    }

    private void SetupRepo(IEnumerable<Product> items, int total)
    {
        _productRepo
            .Setup(r => r.GetPagedAsync(
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(),
                It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, total));
        _mapper
            .Setup(m => m.Map<IEnumerable<ProductListDto>>(It.IsAny<IEnumerable<Product>>()))
            .Returns([]);
    }

    [Fact]
    public async Task Handle_ReturnsPagedList()
    {
        SetupRepo([], 0);
        var query = new GetProductsQuery(null, null, null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(101, 100)]
    [InlineData(200, 100)]
    public async Task Handle_ClampsPageSizeToValidRange(int rawPageSize, int expectedClamped)
    {
        int capturedPageSize = 0;
        _productRepo
            .Setup(r => r.GetPagedAsync(
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool?>(),
                It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<string?, string?, bool?, string?, string, bool, int, int, CancellationToken>(
                (_, _, _, _, _, _, _, ps, _) => capturedPageSize = ps)
            .ReturnsAsync((Enumerable.Empty<Product>(), 0));
        _mapper
            .Setup(m => m.Map<IEnumerable<ProductListDto>>(It.IsAny<IEnumerable<Product>>()))
            .Returns([]);

        await _handler.Handle(new GetProductsQuery(null, null, null, PageSize: rawPageSize), CancellationToken.None);

        capturedPageSize.Should().Be(expectedClamped);
    }

    [Fact]
    public async Task Handle_ForwardsFiltersToRepository()
    {
        SetupRepo([], 0);
        var query = new GetProductsQuery("Gaming", "RTX", true, "NVIDIA", "price", true, 2, 20);

        await _handler.Handle(query, CancellationToken.None);

        _productRepo.Verify(r => r.GetPagedAsync(
            "Gaming", "RTX", true, "NVIDIA", "price", true, 2, 20,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectPaginationMetadata()
    {
        SetupRepo([], 50);
        var query = new GetProductsQuery(null, null, null, Page: 2, PageSize: 10);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(50);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(5);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue(); // page 2 of 5
    }
}
