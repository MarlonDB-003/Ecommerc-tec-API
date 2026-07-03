using Microsoft.AspNetCore.Identity;
using TechWorld.Application.Auth.Commands.Login;
using TechWorld.Application.Auth.Commands.Register;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Infrastructure.Identity;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtService jwtService,
    IUserProfileRepository profileRepository,
    IUnitOfWork unitOfWork) : IAuthService
{
    public async Task<RegisterResponse> RegisterAsync(RegisterCommand command, CancellationToken ct = default)
    {
        var existingUser = await userManager.FindByEmailAsync(command.Email);
        if (existingUser != null)
            throw new ConflictException("Email já está em uso.");

        var user = new ApplicationUser
        {
            Email = command.Email,
            UserName = command.Email,
            DisplayName = command.DisplayName
        };

        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Falha ao criar usuário: {errors}");
        }

        var profile = UserProfile.Create(user.Id, command.DisplayName);
        await profileRepository.AddAsync(profile, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var token = jwtService.GenerateToken(user.Id, user.Email!, user.DisplayName, user.IsAdmin);

        return new RegisterResponse(user.Id, user.Email!, user.DisplayName, token);
    }

    public async Task<LoginResponse> LoginAsync(LoginCommand command, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(command.Email)
            ?? throw new UnauthorizedAccessException("Credenciais inválidas.");

        var result = await signInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        var token = jwtService.GenerateToken(user.Id, user.Email!, user.DisplayName, user.IsAdmin);

        return new LoginResponse(user.Id, user.Email!, user.DisplayName, user.IsAdmin, token);
    }
}
