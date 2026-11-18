namespace Exploration.Application.Images;

public interface IImageReadRepository
{
    /// <summary>Returns the name under which the image file is stored, or null when the image does not exist.</summary>
    Task<string?> GetStoredFileNameAsync(Guid imageId);
}
