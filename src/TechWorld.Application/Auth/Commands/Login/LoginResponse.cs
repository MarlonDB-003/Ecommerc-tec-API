namespace TechWorld.Application.Auth.Commands.Login;

public record LoginResponse(
    Guid UserId,
    string Email,
    string? DisplayName,
    bool IsAdmin,
    string Token
);
