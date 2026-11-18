using Games.Application.Images;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.Images;

internal sealed class ImageReadRepository : IImageReadRepository
{
    private readonly GamesDbContext _dbContext;

    public ImageReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ImageFileDto?> GetByIdAsync(Guid id)
    {
        return _dbContext.Images
            .Where(image => image.Id == id)
            .Select(image => new ImageFileDto(image.StoredFileName))
            .FirstOrDefaultAsync();
    }
}
