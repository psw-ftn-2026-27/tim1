using Exploration.Application.UserPositioning;
using Exploration.Application.UserPositions;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.UserPositions;

internal sealed class UserPositionReadRepository : IUserPositionReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public UserPositionReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CurrentPositionDto?> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.UserPositions
            .Where(position => position.UserId == userId)
            .Select(position => new CurrentPositionDto(position.Latitude, position.Longitude, position.UpdatedAt))
            .SingleOrDefaultAsync();
    }
}
