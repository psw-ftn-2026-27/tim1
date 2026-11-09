using Microsoft.Extensions.Configuration;
using Shared.Domain.Exceptions;
using Social.Application.Images;

namespace Social.Infrastructure.Images;

/// <summary>
/// Keeps image files in one local folder. The folder is read from the configuration key
/// "Social:ImagesFolder"; without it, a folder under the user's local application data.
/// </summary>
internal sealed class LocalImageStorage : IImageStorage
{
    private readonly string _folder;

    public LocalImageStorage(IConfiguration configuration)
    {
        _folder = configuration["Social:ImagesFolder"]
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Explorer",
                "social-images");
    }

    public async Task SaveAsync(string storedFileName, byte[] content)
    {
        Directory.CreateDirectory(_folder);
        await File.WriteAllBytesAsync(PathOf(storedFileName), content);
    }

    public async Task<ImageContentDto> ReadAsync(string storedFileName)
    {
        var path = PathOf(storedFileName);
        if (!File.Exists(path))
        {
            throw new NotFoundException("The image file does not exist.");
        }

        return new ImageContentDto(await File.ReadAllBytesAsync(path), "image/webp");
    }

    public void Delete(string storedFileName)
    {
        File.Delete(PathOf(storedFileName));
    }

    private string PathOf(string storedFileName) => Path.Combine(_folder, storedFileName);
}
