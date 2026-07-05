using FluentValidation.TestHelper;
using TechWorld.Application.Products.Commands.CreateProduct;

namespace TechWorld.UnitTests.Application.Products;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    private static CreateProductCommand ValidCommand() => new(
        Name: "Notebook Dell XPS",
        Price: 5999m,
        Category: "Computadores",
        Description: "Excelente notebook",
        ImageUrl: null,
        Stock: 10,
        DiscountPercentage: 0,
        Specifications: [],
        Brand: "Dell"
    );

    [Fact]
    public void Should_HaveNoErrors_WhenCommandIsValid()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_HaveError_WhenNameIsEmpty(string name)
    {
        var cmd = ValidCommand() with { Name = name };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_WhenNameExceedsMaxLength()
    {
        var cmd = ValidCommand() with { Name = new string('A', 201) };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Should_HaveError_WhenPriceIsNotPositive(decimal price)
    {
        var cmd = ValidCommand() with { Price = price };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Should_HaveNoError_WhenPriceIsPositive()
    {
        var cmd = ValidCommand() with { Price = 0.01m };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_HaveError_WhenCategoryIsEmpty(string category)
    {
        var cmd = ValidCommand() with { Category = category };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Category);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Should_HaveError_WhenDiscountPercentageIsOutOfRange(int discount)
    {
        var cmd = ValidCommand() with { DiscountPercentage = discount };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.DiscountPercentage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Should_HaveNoError_WhenDiscountPercentageIsValid(int discount)
    {
        var cmd = ValidCommand() with { DiscountPercentage = discount };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.DiscountPercentage);
    }

    [Fact]
    public void Should_HaveError_WhenStockIsNegative()
    {
        var cmd = ValidCommand() with { Stock = -1 };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Stock);
    }

    [Fact]
    public void Should_HaveNoError_WhenStockIsZero()
    {
        var cmd = ValidCommand() with { Stock = 0 };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.Stock);
    }

    [Fact]
    public void Should_HaveError_WhenDescriptionExceedsMaxLength()
    {
        var cmd = ValidCommand() with { Description = new string('X', 2001) };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_HaveError_WhenImageUrlExceedsMaxLength()
    {
        var cmd = ValidCommand() with { ImageUrl = new string('X', 501) };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ImageUrl);
    }

    [Fact]
    public void Should_HaveError_WhenBrandExceedsMaxLength()
    {
        var cmd = ValidCommand() with { Brand = new string('X', 101) };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Brand);
    }
}
