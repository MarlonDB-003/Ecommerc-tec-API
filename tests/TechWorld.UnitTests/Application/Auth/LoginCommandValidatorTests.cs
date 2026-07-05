using FluentAssertions;
using FluentValidation.TestHelper;
using TechWorld.Application.Auth.Commands.Login;

namespace TechWorld.UnitTests.Application.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Should_HaveNoErrors_WhenCommandIsValid()
    {
        var command = new LoginCommand("usuario@exemplo.com", "SenhaSegura123");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-email")]
    [InlineData("sem-arroba.com")]
    [InlineData("@dominio.com")]
    public void Should_HaveError_WhenEmailIsInvalid(string email)
    {
        var command = new LoginCommand(email, "SenhaSegura123");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_HaveError_WhenEmailIsEmpty()
    {
        var command = new LoginCommand("", "SenhaSegura123");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_HaveError_WhenPasswordIsEmpty()
    {
        var command = new LoginCommand("usuario@exemplo.com", "");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_HaveNoError_WhenPasswordIsProvided()
    {
        var command = new LoginCommand("usuario@exemplo.com", "qualquersenha");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
