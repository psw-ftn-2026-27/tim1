using Games.Application.LocationChallenges;
using Games.Domain.LocationChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal sealed class LocationChallengeRepository : ILocationChallengeRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<LocationChallenge?> GetByIdAsync(Guid id)
    {
        return _dbContext.LocationChallenges.SingleOrDefaultAsync(challenge => challenge.Id == id);
    }

    public void Add(LocationChallenge challenge)
    {
        _dbContext.LocationChallenges.Add(challenge);
    }

    public void Remove(LocationChallenge challenge)
    {
        _dbContext.LocationChallenges.Remove(challenge);
    }
}
