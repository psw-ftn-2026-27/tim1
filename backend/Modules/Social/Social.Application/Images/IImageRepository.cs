using Social.Domain.Images;

namespace Social.Application.Images;

public interface IImageRepository
{
    Task<List<Image>> GetByIdsAsync(List<Guid> ids);
    Task<List<Image>> GetUnattachedAsync();
    void Add(Image image);
    void Remove(Image image);
}
