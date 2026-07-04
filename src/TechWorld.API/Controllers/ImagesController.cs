using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWorld.Application.Common.Interfaces;

namespace TechWorld.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class ImagesController(IImageStorageService imageStorageService) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile image, CancellationToken ct)
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

        return Ok(new { url });
    }
}
