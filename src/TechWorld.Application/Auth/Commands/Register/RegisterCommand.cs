using MediatR;

namespace TechWorld.Application.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string? DisplayName
) : IRequest<RegisterResponse>;
