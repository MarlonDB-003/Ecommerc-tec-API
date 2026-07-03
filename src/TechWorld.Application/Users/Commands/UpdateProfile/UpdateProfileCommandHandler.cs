using MediatR;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Application.Users.Queries.GetMyProfile;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Application.Users.Commands.UpdateProfile;

public class UpdateProfileCommandHandler(
    IUserProfileRepository profileRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser) : IRequestHandler<UpdateProfileCommand, UserProfileDto>
{
    public async Task<UserProfileDto> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        if (currentUser.UserId is null)
            throw new ForbiddenException();

        var profile = await profileRepository.GetByUserIdAsync(currentUser.UserId.Value, ct);

        if (profile is null)
        {
            profile = UserProfile.Create(currentUser.UserId.Value, request.DisplayName);
            profile.Update(request.DisplayName, request.Phone, request.AvatarUrl);
            await profileRepository.AddAsync(profile, ct);
        }
        else
        {
            profile.Update(request.DisplayName, request.Phone, request.AvatarUrl);
            profileRepository.Update(profile);
        }

        await unitOfWork.SaveChangesAsync(ct);

        return new UserProfileDto(
            currentUser.UserId.Value,
            currentUser.Email ?? string.Empty,
            profile.DisplayName,
            profile.Phone,
            profile.AvatarUrl,
            currentUser.IsAdmin);
    }
}
