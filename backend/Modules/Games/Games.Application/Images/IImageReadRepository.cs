namespace Games.Application.Images;

public interface IImageReadRepository
{
    Task<ImageFileDto?> GetByIdAsync(Guid id);
}
