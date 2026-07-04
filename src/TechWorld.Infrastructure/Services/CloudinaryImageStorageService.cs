using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using TechWorld.Application.Common.Interfaces;

namespace TechWorld.Infrastructure.Services;

public class CloudinaryImageStorageService : IImageStorageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryImageStorageService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"] ?? throw new InvalidOperationException("Cloudinary:CloudName não configurado.");
        var apiKey = configuration["Cloudinary:ApiKey"] ?? throw new InvalidOperationException("Cloudinary:ApiKey não configurado.");
        var apiSecret = configuration["Cloudinary:ApiSecret"] ?? throw new InvalidOperationException("Cloudinary:ApiSecret não configurado.");

        _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret))
        {
            Api = { Secure = true }
        };
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = "techworld/products",
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error is not null)
            throw new InvalidOperationException($"Erro ao fazer upload da imagem: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }
}
