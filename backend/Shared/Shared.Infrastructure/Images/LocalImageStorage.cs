using Microsoft.Extensions.Configuration;

namespace Shared.Infrastructure.Images;

/// <summary>
/// Keeps image files on the local disk, in one subfolder per module under the root folder
/// from the configuration key "Images:RootFolder" (by default "Explorer/images" in the
/// user's local application data). The file name comes from the module's image record and
/// never from the uploaded file (P8); a name that is not a plain file name is a bug in the
/// calling module and throws ArgumentException.
/// </summary>
public sealed class LocalImageStorage
{
    public const string RootFolderKey = "Images:RootFolder";

    private readonly string _folder;

    public LocalImageStorage(IConfiguration configuration, string moduleFolder)
    {
        EnsurePlainName(moduleFolder);

        var root = configuration[RootFolderKey];
        if (string.IsNullOrWhiteSpace(root))
        {
            root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Explorer",
                "images");
        }

        _folder = Path.Combine(root, moduleFolder);
    }

    public async Task SaveAsync(string fileName, byte[] content)
    {
        Directory.CreateDirectory(_folder);
        await File.WriteAllBytesAsync(PathOf(fileName), content);
    }

    /// <summary>Returns null when the file does not exist.</summary>
    public async Task<byte[]?> ReadAsync(string fileName)
    {
        var path = PathOf(fileName);
        if (!File.Exists(path))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(path);
    }

    /// <summary>Deleting a file that is already gone is not an error, so a cleanup can safely run again.</summary>
    public void Delete(string fileName)
    {
        var path = PathOf(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private string PathOf(string fileName)
    {
        EnsurePlainName(fileName);
        return Path.Combine(_folder, fileName);
    }

    private static void EnsurePlainName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name != Path.GetFileName(name) || name is "." or "..")
        {
            throw new ArgumentException($"'{name}' is not a plain file or folder name.", nameof(name));
        }
    }
}
