using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TechWorld.Application.Common.Interfaces;

namespace TechWorld.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var id = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var guid) ? guid : null;
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAdmin =>
        bool.TryParse(User?.FindFirstValue("isAdmin"), out var isAdmin) && isAdmin;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
