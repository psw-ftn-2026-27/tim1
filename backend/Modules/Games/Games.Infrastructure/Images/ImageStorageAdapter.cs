using Games.Application.Images;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.Images;

namespace Games.Infrastructure.Images;

/// <summary>Implements Games' storage port with the shared storage, in Games' own subfolder.</summary>
internal sealed class ImageStorageAdapter : IImageStorage
{
    public const string ModuleFolder = "games";

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
}
