using FluentValidation;

namespace TechWorld.Application.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email inválido.");
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres.")
            .Matches(@"\d").WithMessage("Senha deve conter pelo menos um número.")
            .Matches(@"[A-Z]").WithMessage("Senha deve conter pelo menos uma letra maiúscula.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Senha deve conter pelo menos um caractere especial.");
        RuleFor(x => x.DisplayName).MaximumLength(200).When(x => x.DisplayName != null);
    }
}
