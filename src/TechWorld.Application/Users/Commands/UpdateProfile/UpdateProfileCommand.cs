using MediatR;
using TechWorld.Application.Users.Queries.GetMyProfile;

namespace TechWorld.Application.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(
    string? DisplayName,
    string? Phone,
    string? AvatarUrl
) : IRequest<UserProfileDto>;
