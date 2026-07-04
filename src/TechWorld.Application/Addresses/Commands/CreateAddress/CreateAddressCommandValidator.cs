using FluentValidation;

namespace TechWorld.Application.Addresses.Commands.CreateAddress;

public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
{
    public CreateAddressCommandValidator()
    {
        RuleFor(x => x.Cep)
            .NotEmpty().WithMessage("CEP é obrigatório.")
            .Matches(@"^\d{5}-?\d{3}$").WithMessage("CEP deve estar no formato 00000-000.");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Logradouro é obrigatório.")
            .MaximumLength(200).WithMessage("Logradouro deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Número é obrigatório.")
            .MaximumLength(20).WithMessage("Número deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Neighborhood)
            .NotEmpty().WithMessage("Bairro é obrigatório.")
            .MaximumLength(100).WithMessage("Bairro deve ter no máximo 100 caracteres.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Cidade é obrigatória.")
            .MaximumLength(100).WithMessage("Cidade deve ter no máximo 100 caracteres.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("Estado é obrigatório.")
            .Length(2).WithMessage("Estado deve ser a sigla com 2 letras.")
            .Matches(@"^[A-Z]{2}$").WithMessage("Estado deve conter apenas letras maiúsculas (ex: SP).");

        RuleFor(x => x.Label)
            .MaximumLength(50).WithMessage("Rótulo deve ter no máximo 50 caracteres.")
            .When(x => x.Label != null);

        RuleFor(x => x.Complement)
            .MaximumLength(100).WithMessage("Complemento deve ter no máximo 100 caracteres.")
            .When(x => x.Complement != null);
    }
}
