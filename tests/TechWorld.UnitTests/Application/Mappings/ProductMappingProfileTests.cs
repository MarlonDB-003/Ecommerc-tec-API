using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using TechWorld.Application.Common.Mappings;
using TechWorld.Application.Products.Queries.GetProductById;
using TechWorld.Application.Products.Queries.GetProducts;
using TechWorld.Domain.Entities;

namespace TechWorld.UnitTests.Application.Mappings;

public class ProductMappingProfileTests
{
    private static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(ProductMappingProfile).Assembly));
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    [Fact]
    public void ConfigurationIsValid()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(ProductMappingProfile).Assembly));
        var mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_Product_To_ProductDetailDto_MapsAllFields()
    {
        var mapper = CreateMapper();
        var product = Product.Create("Monitor 4K", 2500m, "Computadores",
            "Resolução 4K", "https://img.example.com/monitor.jpg",
            stock: 5, discountPercentage: 10, brand: "Dell");

        var dto = mapper.Map<ProductDetailDto>(product);

        dto.Id.Should().Be(product.Id);
        dto.Name.Should().Be("Monitor 4K");
        dto.Price.Should().Be(2500m);
        dto.Category.Should().Be("Computadores");
        dto.Description.Should().Be("Resolução 4K");
        dto.ImageUrl.Should().Be("https://img.example.com/monitor.jpg");
        dto.Stock.Should().Be(5);
        dto.DiscountPercentage.Should().Be(10);
        dto.Brand.Should().Be("Dell");
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Map_Product_To_ProductDetailDto_ComputesDiscountedPrice()
    {
        var mapper = CreateMapper();
        var product = Product.Create("Monitor 4K", 1000m, "Computadores", discountPercentage: 20);

        var dto = mapper.Map<ProductDetailDto>(product);

        dto.DiscountedPrice.Should().Be(800m);
    }

    [Fact]
    public void Map_Product_To_ProductDetailDto_NoDiscount_DiscountedPriceEqualsPrice()
    {
        var mapper = CreateMapper();
        var product = Product.Create("Monitor 4K", 1000m, "Computadores");

        var dto = mapper.Map<ProductDetailDto>(product);

        dto.DiscountedPrice.Should().Be(1000m);
    }

    [Fact]
    public void Map_Product_To_ProductListDto_MapsAllFields()
    {
        var mapper = CreateMapper();
        var product = Product.Create("RTX 4090", 8000m, "Componentes",
            stock: 3, discountPercentage: 5, brand: "NVIDIA");

        var dto = mapper.Map<ProductListDto>(product);

        dto.Id.Should().Be(product.Id);
        dto.Name.Should().Be("RTX 4090");
        dto.Price.Should().Be(8000m);
        dto.Stock.Should().Be(3);
        dto.DiscountPercentage.Should().Be(5);
        dto.Brand.Should().Be("NVIDIA");
    }

    [Fact]
    public void Map_Product_To_ProductListDto_ComputesDiscountedPrice()
    {
        var mapper = CreateMapper();
        var product = Product.Create("RTX 4090", 8000m, "Componentes", discountPercentage: 25);

        var dto = mapper.Map<ProductListDto>(product);

        dto.DiscountedPrice.Should().Be(6000m);
    }

    [Fact]
    public void Map_ProductSpecification_To_SpecificationDto_MapsFields()
    {
        var mapper = CreateMapper();
        var productId = Guid.NewGuid();
        var spec = ProductSpecification.Create(productId, "RAM", "32GB", 1);

        var dto = mapper.Map<SpecificationDto>(spec);

        dto.Id.Should().Be(spec.Id);
        dto.Label.Should().Be("RAM");
        dto.Value.Should().Be("32GB");
        dto.DisplayOrder.Should().Be(1);
    }

    [Fact]
    public void Map_ProductWithSpecifications_To_ProductDetailDto_IncludesSpecs()
    {
        var mapper = CreateMapper();
        var product = Product.Create("PC Gamer", 5000m, "Computadores");
        var specs = new List<ProductSpecification>
        {
            ProductSpecification.Create(product.Id, "CPU", "i9-13900K"),
            ProductSpecification.Create(product.Id, "RAM", "32GB DDR5"),
        };
        product.SetSpecifications(specs);

        var dto = mapper.Map<ProductDetailDto>(product);

        dto.Specifications.Should().HaveCount(2);
        dto.Specifications.First().Label.Should().Be("CPU");
    }
}
