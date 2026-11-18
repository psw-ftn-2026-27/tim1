using Games.Application.Images;
using Games.Domain.Images;

namespace Games.Infrastructure.Persistence.Images;

internal sealed class ImageRepository : IImageRepository
{
    private readonly GamesDbContext _dbContext;

    public ImageRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(Image image)
    {
        _dbContext.Images.Add(image);
    }
}
