using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using TechWorld.Application.Common.Interfaces;

namespace TechWorld.Infrastructure.Services;

public class CloudinaryImageStorageService : IImageStorageService
{
    private readonly Cloudinary? _cloudinary;

    public CloudinaryImageStorageService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        if (!string.IsNullOrEmpty(cloudName) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiSecret))
        {
            _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret))
            {
                Api = { Secure = true }
            };
        }
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        if (_cloudinary is null)
            throw new InvalidOperationException("Cloudinary não configurado. Defina Cloudinary:CloudName, ApiKey e ApiSecret.");

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
