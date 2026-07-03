namespace TechWorld.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId, string email, string? displayName, bool isAdmin);
}
