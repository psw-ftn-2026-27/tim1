using Microsoft.EntityFrameworkCore;
using Social.Application.Images;
using Social.Domain.Images;

namespace Social.Infrastructure.Persistence.Images;

internal sealed class ImageRepository : IImageRepository
{
    private readonly SocialDbContext _dbContext;

    public ImageRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Image>> GetByIdsAsync(List<Guid> ids)
    {
        return _dbContext.Images.Where(image => ids.Contains(image.Id)).ToListAsync();
    }

    public Task<List<Image>> GetUnattachedAsync()
    {
        return _dbContext.Images.Where(image => image.Status == ImageStatus.Unattached).ToListAsync();
    }

    public void Add(Image image)
    {
        _dbContext.Images.Add(image);
    }

    public void Remove(Image image)
    {
        _dbContext.Images.Remove(image);
    }
}
