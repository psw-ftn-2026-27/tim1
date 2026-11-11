using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.Images;
using Social.Application.Images;

namespace Social.Infrastructure.Images;

/// <summary>Implements Social's storage port with the shared storage, in Social's own subfolder.</summary>
internal sealed class ImageStorageAdapter : IImageStorage
{
    public const string ModuleFolder = "social";

    private readonly LocalImageStorage _storage;

    public ImageStorageAdapter(IConfiguration configuration)
    {
        _storage = new LocalImageStorage(configuration, ModuleFolder);
    }

    public Task SaveAsync(string storedFileName, byte[] content)
    {
        return _storage.SaveAsync(storedFileName, content);
    }

    public async Task<ImageContentDto?> ReadAsync(string storedFileName)
    {
        var content = await _storage.ReadAsync(storedFileName);
        return content is null ? null : new ImageContentDto(content, ImageProcessor.ContentType);
    }

    public void Delete(string storedFileName)
    {
        _storage.Delete(storedFileName);
    }
}
