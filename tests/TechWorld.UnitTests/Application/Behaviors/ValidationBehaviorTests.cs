using FluentValidation;
using FluentValidation.Results;
using MediatR;
using TechWorld.Application.Common.Behaviors;

namespace TechWorld.UnitTests.Application.Behaviors;

public record ValidationTestRequest(string Value) : IRequest<string>;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithNoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<ValidationTestRequest, string>([]);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = ct => { nextCalled = true; return Task.FromResult("ok"); };

        var result = await behavior.Handle(new ValidationTestRequest("x"), next, CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WhenValidationPasses_CallsNext()
    {
        var validator = new Mock<IValidator<ValidationTestRequest>>();
        validator
            .Setup(v => v.Validate(It.IsAny<ValidationContext<ValidationTestRequest>>()))
            .Returns(new ValidationResult());

        var behavior = new ValidationBehavior<ValidationTestRequest, string>([validator.Object]);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = ct => { nextCalled = true; return Task.FromResult("ok"); };

        await behavior.Handle(new ValidationTestRequest("x"), next, CancellationToken.None);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ThrowsValidationException()
    {
        var failures = new List<ValidationFailure> { new("Value", "Value is required") };
        var validator = new Mock<IValidator<ValidationTestRequest>>();
        validator
            .Setup(v => v.Validate(It.IsAny<ValidationContext<ValidationTestRequest>>()))
            .Returns(new ValidationResult(failures));

        var behavior = new ValidationBehavior<ValidationTestRequest, string>([validator.Object]);
        RequestHandlerDelegate<string> next = ct => Task.FromResult("ok");

        var act = async () => await behavior.Handle(new ValidationTestRequest(""), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenMultipleValidatorsAndOneFails_ThrowsValidationException()
    {
        var passing = new Mock<IValidator<ValidationTestRequest>>();
        passing
            .Setup(v => v.Validate(It.IsAny<ValidationContext<ValidationTestRequest>>()))
            .Returns(new ValidationResult());

        var failing = new Mock<IValidator<ValidationTestRequest>>();
        failing
            .Setup(v => v.Validate(It.IsAny<ValidationContext<ValidationTestRequest>>()))
            .Returns(new ValidationResult([new ValidationFailure("Value", "Too short")]));

        var behavior = new ValidationBehavior<ValidationTestRequest, string>([passing.Object, failing.Object]);
        RequestHandlerDelegate<string> next = ct => Task.FromResult("ok");

        var act = async () => await behavior.Handle(new ValidationTestRequest(""), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenMultipleValidatorsAllPass_CallsNextOnce()
    {
        var v1 = new Mock<IValidator<ValidationTestRequest>>();
        v1.Setup(v => v.Validate(It.IsAny<ValidationContext<ValidationTestRequest>>())).Returns(new ValidationResult());
        var v2 = new Mock<IValidator<ValidationTestRequest>>();
        v2.Setup(v => v.Validate(It.IsAny<ValidationContext<ValidationTestRequest>>())).Returns(new ValidationResult());

        var behavior = new ValidationBehavior<ValidationTestRequest, string>([v1.Object, v2.Object]);
        var callCount = 0;
        RequestHandlerDelegate<string> next = ct => { callCount++; return Task.FromResult("ok"); };

        await behavior.Handle(new ValidationTestRequest("x"), next, CancellationToken.None);

        callCount.Should().Be(1);
    }
}
