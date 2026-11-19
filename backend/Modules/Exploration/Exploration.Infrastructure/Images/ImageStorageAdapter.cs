using Exploration.Application.Images;
using Microsoft.Extensions.Configuration;
using Shared.Infrastructure.Images;

namespace Exploration.Infrastructure.Images;

/// <summary>Implements Exploration's storage port with the shared storage, in Exploration's own subfolder.</summary>
internal sealed class ImageStorageAdapter : IImageStorage
{
    public const string ModuleFolder = "exploration";

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
