namespace TechWorld.Application.Common.Interfaces;

public interface IImageStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
