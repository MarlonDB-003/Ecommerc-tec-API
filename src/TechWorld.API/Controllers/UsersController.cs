using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWorld.Application.Common.Interfaces;
using TechWorld.Application.Users.Commands.UpdateProfile;
using TechWorld.Application.Users.Queries.GetMyProfile;

namespace TechWorld.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IMediator mediator, IImageStorageService imageStorageService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyProfileQuery(), ct);
        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("me/avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile image, CancellationToken ct)
    {
        if (image is null || image.Length == 0)
            return BadRequest(new { detail = "Nenhuma imagem enviada." });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(image.ContentType.ToLower()))
            return BadRequest(new { detail = "Formato inválido. Use JPEG, PNG, WebP ou GIF." });

        if (image.Length > 5 * 1024 * 1024)
            return BadRequest(new { detail = "Imagem muito grande. Tamanho máximo: 5MB." });

        await using var stream = image.OpenReadStream();
        var url = await imageStorageService.UploadAsync(stream, image.FileName, ct);

        var current = await mediator.Send(new GetMyProfileQuery(), ct);
        var updated = await mediator.Send(
            new UpdateProfileCommand(current.DisplayName, current.Phone, url), ct);

        return Ok(updated);
    }
}
