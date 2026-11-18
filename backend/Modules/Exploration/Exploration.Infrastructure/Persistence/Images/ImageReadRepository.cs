using Exploration.Application.Images;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.Images;

internal sealed class ImageReadRepository : IImageReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public ImageReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<string?> GetStoredFileNameAsync(Guid imageId)
    {
        return _dbContext.Images
            .AsNoTracking()
            .Where(image => image.Id == imageId)
            .Select(image => image.StoredFileName)
            .FirstOrDefaultAsync();
    }
}
