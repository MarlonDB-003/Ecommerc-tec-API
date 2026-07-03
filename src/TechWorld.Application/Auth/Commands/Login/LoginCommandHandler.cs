using MediatR;
using TechWorld.Application.Common.Interfaces;

namespace TechWorld.Application.Auth.Commands.Login;

public class LoginCommandHandler(IAuthService authService) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken ct) =>
        await authService.LoginAsync(request, ct);
}
