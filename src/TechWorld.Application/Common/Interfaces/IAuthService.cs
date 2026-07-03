using TechWorld.Application.Auth.Commands.Login;
using TechWorld.Application.Auth.Commands.Register;

namespace TechWorld.Application.Common.Interfaces;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterCommand command, CancellationToken ct = default);
    Task<LoginResponse> LoginAsync(LoginCommand command, CancellationToken ct = default);
}
