namespace TechWorld.Application.Auth.Commands.Register;

public record RegisterResponse(
    Guid UserId,
    string Email,
    string? DisplayName,
    string Token
);
