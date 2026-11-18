namespace Games.Application.Images;

/// <summary>Keeps the files of Games' images. Implemented in Infrastructure over the shared image storage.</summary>
public interface IImageStorage
{
    Task SaveAsync(string storedFileName, byte[] content);

    /// <summary>Returns null when the file does not exist.</summary>
    Task<ImageContentDto?> ReadAsync(string storedFileName);
}
