using Games.Application.ChallengeCompletions;
using Games.Domain.ChallengeCompletions;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.ChallengeCompletions;

internal sealed class ChallengeCompletionRepository : IChallengeCompletionRepository
{
    private readonly GamesDbContext _dbContext;

    public ChallengeCompletionRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ChallengeCompletion?> GetByPlayerAndChallengeAsync(Guid playerId, Guid challengeId)
    {
        return _dbContext.ChallengeCompletions
            .SingleOrDefaultAsync(completion => completion.PlayerId == playerId && completion.ChallengeId == challengeId);
    }

    public void Add(ChallengeCompletion completion)
    {
        _dbContext.ChallengeCompletions.Add(completion);
    }
}
