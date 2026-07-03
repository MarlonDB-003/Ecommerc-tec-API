using Microsoft.EntityFrameworkCore;
using TechWorld.Domain.Entities;
using TechWorld.Domain.Interfaces.Repositories;

namespace TechWorld.Infrastructure.Persistence.Repositories;

public class UserProfileRepository(ApplicationDbContext db) : IUserProfileRepository
{
    public async Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);

    public async Task AddAsync(UserProfile profile, CancellationToken ct = default) =>
        await db.UserProfiles.AddAsync(profile, ct);

    public void Update(UserProfile profile) => db.UserProfiles.Update(profile);
}
