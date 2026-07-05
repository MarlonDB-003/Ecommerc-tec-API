using FluentAssertions;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Exceptions;

namespace TechWorld.UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ReturnsProduct()
    {
        var product = Product.Create("iPhone 15", 5999m, "Smartphones");

        product.Name.Should().Be("iPhone 15");
        product.Price.Should().Be(5999m);
        product.Category.Should().Be("Smartphones");
        product.IsActive.Should().BeTrue();
        product.Stock.Should().Be(0);
        product.DiscountPercentage.Should().Be(0);
        product.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ThrowsDomainException(string name)
    {
        var act = () => Product.Create(name, 5999m, "Smartphones");
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Create_WithNonPositivePrice_ThrowsDomainException(decimal price)
    {
        var act = () => Product.Create("iPhone 15", price, "Smartphones");
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(200)]
    public void Create_WithInvalidDiscountPercentage_ThrowsDomainException(int discount)
    {
        var act = () => Product.Create("iPhone 15", 5999m, "Smartphones", discountPercentage: discount);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Create_WithValidDiscountPercentage_DoesNotThrow(int discount)
    {
        var act = () => Product.Create("iPhone 15", 5999m, "Smartphones", discountPercentage: discount);
        act.Should().NotThrow();
    }

    [Fact]
    public void CalculateDiscountedPrice_WithZeroDiscount_ReturnsFullPrice()
    {
        var product = Product.Create("iPhone 15", 5000m, "Smartphones");
        product.CalculateDiscountedPrice().Should().Be(5000m);
    }

    [Fact]
    public void CalculateDiscountedPrice_With20PercentDiscount_ReturnsCorrectPrice()
    {
        var product = Product.Create("iPhone 15", 1000m, "Smartphones", discountPercentage: 20);
        product.CalculateDiscountedPrice().Should().Be(800m);
    }

    [Fact]
    public void CalculateDiscountedPrice_With100PercentDiscount_ReturnsZero()
    {
        var product = Product.Create("Free Item", 100m, "Smartphones", discountPercentage: 100);
        product.CalculateDiscountedPrice().Should().Be(0m);
    }

    [Fact]
    public void UpdateStock_WithPositiveDelta_IncreasesStock()
    {
        var product = Product.Create("iPhone 15", 5000m, "Smartphones", stock: 10);
        product.UpdateStock(5);
        product.Stock.Should().Be(15);
    }

    [Fact]
    public void UpdateStock_WithNegativeDelta_DecreasesStock()
    {
        var product = Product.Create("iPhone 15", 5000m, "Smartphones", stock: 10);
        product.UpdateStock(-5);
        product.Stock.Should().Be(5);
    }

    [Fact]
    public void UpdateStock_ThatResultsInNegativeStock_ThrowsDomainException()
    {
        var product = Product.Create("iPhone 15", 5000m, "Smartphones", stock: 5);
        var act = () => product.UpdateStock(-10);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateStock_ToExactlyZero_DoesNotThrow()
    {
        var product = Product.Create("iPhone 15", 5000m, "Smartphones", stock: 5);
        var act = () => product.UpdateStock(-5);
        act.Should().NotThrow();
        product.Stock.Should().Be(0);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var product = Product.Create("iPhone 15", 5000m, "Smartphones");
        product.Deactivate();
        product.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Update_WithValidData_UpdatesAllProperties()
    {
        var product = Product.Create("iPhone 15", 5000m, "Smartphones");

        product.Update("iPhone 16", 6000m, "Gaming", "Desc", null, 10, 15, "Apple");

        product.Name.Should().Be("iPhone 16");
        product.Price.Should().Be(6000m);
        product.Category.Should().Be("Gaming");
        product.Description.Should().Be("Desc");
        product.Stock.Should().Be(10);
        product.DiscountPercentage.Should().Be(15);
        product.Brand.Should().Be("Apple");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyName_ThrowsDomainException(string name)
    {
        var product = Product.Create("iPhone 15", 5000m, "Smartphones");
        var act = () => product.Update(name, 6000m, "Smartphones", null, null, 0, 0, null);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Update_WithInvalidPrice_ThrowsDomainException(decimal price)
    {
        var product = Product.Create("iPhone 15", 5000m, "Smartphones");
        var act = () => product.Update("iPhone 15", price, "Smartphones", null, null, 0, 0, null);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Update_WithInvalidDiscount_ThrowsDomainException(int discount)
    {
        var product = Product.Create("iPhone 15", 5000m, "Smartphones");
        var act = () => product.Update("iPhone 15", 5000m, "Smartphones", null, null, 0, discount, null);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void SetSpecifications_ReplacesAllSpecifications()
    {
        var product = Product.Create("iPhone 15", 5000m, "Smartphones");
        var initial = new List<ProductSpecification>
        {
            ProductSpecification.Create(product.Id, "RAM", "8GB"),
        };
        product.SetSpecifications(initial);
        product.Specifications.Should().HaveCount(1);

        var replacement = new List<ProductSpecification>
        {
            ProductSpecification.Create(product.Id, "RAM", "16GB"),
            ProductSpecification.Create(product.Id, "Storage", "512GB"),
        };
        product.SetSpecifications(replacement);
        product.Specifications.Should().HaveCount(2);
        product.Specifications.First().Label.Should().Be("RAM");
        product.Specifications.First().Value.Should().Be("16GB");
    }

    [Fact]
    public void Create_WithOptionalParams_SetsDefaults()
    {
        var product = Product.Create("Monitor", 1500m, "Computadores");
        product.Description.Should().BeNull();
        product.ImageUrl.Should().BeNull();
        product.Brand.Should().BeNull();
        product.Stock.Should().Be(0);
        product.DiscountPercentage.Should().Be(0);
    }
}
