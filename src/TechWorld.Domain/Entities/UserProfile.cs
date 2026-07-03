using TechWorld.Domain.Common;

namespace TechWorld.Domain.Entities;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; private set; }
    public string? DisplayName { get; private set; }
    public string? Phone { get; private set; }
    public string? AvatarUrl { get; private set; }

    protected UserProfile() { }

    public static UserProfile Create(Guid userId, string? displayName = null) =>
        new() { UserId = userId, DisplayName = displayName };

    public void Update(string? displayName, string? phone, string? avatarUrl)
    {
        DisplayName = displayName;
        Phone = phone;
        AvatarUrl = avatarUrl;
        SetUpdatedAt();
    }
}
