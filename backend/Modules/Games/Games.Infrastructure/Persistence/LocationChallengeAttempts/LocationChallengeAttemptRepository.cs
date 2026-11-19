using Games.Application.LocationChallengeAttempts;
using Games.Domain.LocationChallengeAttempts;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallengeAttempts;

internal sealed class LocationChallengeAttemptRepository : ILocationChallengeAttemptRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeAttemptRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<LocationChallengeAttempt?> GetAsync(Guid challengeId, Guid userId)
    {
        return _dbContext.LocationChallengeAttempts
            .SingleOrDefaultAsync(attempt => attempt.ChallengeId == challengeId && attempt.UserId == userId);
    }

    public void Add(LocationChallengeAttempt attempt)
    {
        _dbContext.LocationChallengeAttempts.Add(attempt);
    }
}
