using MediatR;

namespace TechWorld.Application.Users.Queries.GetMyProfile;

public record GetMyProfileQuery : IRequest<UserProfileDto>;

public record UserProfileDto(
    Guid UserId,
    string Email,
    string? DisplayName,
    string? Phone,
    string? AvatarUrl,
    bool IsAdmin
);
