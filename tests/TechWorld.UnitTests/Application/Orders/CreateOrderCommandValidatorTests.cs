using FluentValidation.TestHelper;
using TechWorld.Application.Orders.Commands.CreateOrder;
using TechWorld.Domain.Enums;

namespace TechWorld.UnitTests.Application.Orders;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    private static CreateOrderCommand ValidCommand() => new(
        Items: [new OrderItemInput(Guid.NewGuid(), 2)],
        CustomerInfo: new CustomerInfoInput("Maria Silva", "maria@exemplo.com", "21999999999"),
        AddressInfo: new AddressInfoInput("22041-001", "Rua das Flores", "100", null, "Tijuca", "Rio de Janeiro", "RJ"),
        PaymentMethod: PaymentMethod.CreditCard,
        Installments: 3
    );

    [Fact]
    public void Should_HaveNoErrors_WhenCommandIsValid()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_WhenItemsIsEmpty()
    {
        var cmd = ValidCommand() with { Items = [] };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void Should_HaveError_WhenItemQuantityIsZero()
    {
        var cmd = ValidCommand() with { Items = [new OrderItemInput(Guid.NewGuid(), 0)] };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_HaveError_WhenItemQuantityIsNegative()
    {
        var cmd = ValidCommand() with { Items = [new OrderItemInput(Guid.NewGuid(), -1)] };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_HaveError_WhenCustomerNameIsEmpty()
    {
        var cmd = ValidCommand() with { CustomerInfo = new CustomerInfoInput("", "maria@ex.com", "21999999999") };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-email")]
    public void Should_HaveError_WhenCustomerEmailIsInvalid(string email)
    {
        var cmd = ValidCommand() with { CustomerInfo = new CustomerInfoInput("Maria", email, "21999999999") };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_HaveError_WhenCustomerPhoneIsEmpty()
    {
        var cmd = ValidCommand() with { CustomerInfo = new CustomerInfoInput("Maria", "maria@ex.com", "") };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_HaveError_WhenCepIsEmpty()
    {
        var cmd = ValidCommand() with
        {
            AddressInfo = new AddressInfoInput("", "Rua das Flores", "100", null, "Tijuca", "Rio de Janeiro", "RJ")
        };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_HaveError_WhenStateIsNotTwoChars()
    {
        var cmd = ValidCommand() with
        {
            AddressInfo = new AddressInfoInput("22041-001", "Rua das Flores", "100", null, "Tijuca", "Rio de Janeiro", "Rio")
        };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_HaveError_WhenStateIsEmpty()
    {
        var cmd = ValidCommand() with
        {
            AddressInfo = new AddressInfoInput("22041-001", "Rua das Flores", "100", null, "Tijuca", "Rio de Janeiro", "")
        };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Should_HaveError_WhenInstallmentsIsOutOfRange(int installments)
    {
        var cmd = ValidCommand() with { Installments = installments };
        var result = _validator.TestValidate(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void Should_HaveNoError_WhenInstallmentsIsInRange(int installments)
    {
        var cmd = ValidCommand() with { Installments = installments };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.Installments);
    }
}
