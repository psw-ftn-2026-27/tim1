using Exploration.Application.UserPositions;
using Exploration.Domain.UserPositions;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.UserPositions;

internal sealed class UserPositionRepository : IUserPositionRepository
{
    private readonly ExplorationDbContext _dbContext;

    public UserPositionRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserPosition?> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.UserPositions.SingleOrDefaultAsync(position => position.UserId == userId);
    }

    public void Add(UserPosition position)
    {
        _dbContext.UserPositions.Add(position);
    }

    public void UpdateCoordinates(UserPosition position)
    {
        // Persist the pair together even when EF sees only one coordinate change.
        var entry = _dbContext.Entry(position);
        entry.Property(current => current.Latitude).IsModified = true;
        entry.Property(current => current.Longitude).IsModified = true;
        entry.Property(current => current.UpdatedAt).IsModified = true;
    }
}
