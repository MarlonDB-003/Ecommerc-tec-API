using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TechWorld.Application.Auth.Commands.Login;
using TechWorld.Application.Auth.Commands.Register;

namespace TechWorld.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator, IWebHostEnvironment env) : ControllerBase
{
    private CookieOptions AuthCookieOptions => new()
    {
        HttpOnly = true,
        Secure = !env.IsDevelopment(),
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddHours(1),
        Path = "/"
    };

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        Response.Cookies.Append("access_token", result.Token, AuthCookieOptions);
        return Created(string.Empty, new { result.UserId, result.Email, result.DisplayName });
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        Response.Cookies.Append("access_token", result.Token, AuthCookieOptions);
        return Ok(new { result.UserId, result.Email, result.DisplayName, result.IsAdmin });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.Cookies.Append("access_token", "", new CookieOptions
        {
            HttpOnly = true,
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UnixEpoch,
            Path = "/"
        });
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var displayName = User.FindFirstValue("displayName");
        var isAdmin = User.FindFirstValue("isAdmin") == "true";

        return Ok(new { userId, email, displayName, isAdmin });
    }
}
