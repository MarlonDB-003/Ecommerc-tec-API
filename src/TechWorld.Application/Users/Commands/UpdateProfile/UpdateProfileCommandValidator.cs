using FluentValidation;

namespace TechWorld.Application.Users.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.")
            .When(x => x.DisplayName != null);

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres.")
            .Matches(@"^\+?[\d\s\-\(\)]+$").WithMessage("Formato de telefone inválido.")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.AvatarUrl)
            .Must(BeACloudinaryUrl).WithMessage("URL do avatar deve ser do Cloudinary.")
            .When(x => !string.IsNullOrEmpty(x.AvatarUrl));
    }

    private static bool BeACloudinaryUrl(string? url)
    {
        if (url is null) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var u) &&
               (u.Host.EndsWith(".cloudinary.com") || u.Host == "res.cloudinary.com");
    }
}
