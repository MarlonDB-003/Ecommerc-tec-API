using MediatR;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Application.Users.Queries.GetMyProfile;

public class GetMyProfileQueryHandler(
    IUserProfileRepository profileRepository,
    ICurrentUserService currentUser) : IRequestHandler<GetMyProfileQuery, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(GetMyProfileQuery request, CancellationToken ct)
    {
        if (currentUser.UserId is null)
            throw new ForbiddenException();

        var profile = await profileRepository.GetByUserIdAsync(currentUser.UserId.Value, ct);

        return new UserProfileDto(
            currentUser.UserId.Value,
            currentUser.Email ?? string.Empty,
            profile?.DisplayName,
            profile?.Phone,
            profile?.AvatarUrl,
            currentUser.IsAdmin);
    }
}
