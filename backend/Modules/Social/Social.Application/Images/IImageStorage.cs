namespace Social.Application.Images;

public interface IImageStorage
{
    Task SaveAsync(string storedFileName, byte[] content);
    Task<ImageContentDto> ReadAsync(string storedFileName);
    void Delete(string storedFileName);
}
