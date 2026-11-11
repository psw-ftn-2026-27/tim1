using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal sealed class SocialChallengeRepository : ISocialChallengeRepository
{
    private readonly GamesDbContext _dbContext;

    public SocialChallengeRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SocialChallenge?> GetByIdAsync(Guid id)
    {
        return _dbContext.SocialChallenges.FirstOrDefaultAsync(challenge => challenge.Id == id);
    }

    public void Add(SocialChallenge challenge)
    {
        _dbContext.SocialChallenges.Add(challenge);
    }

    public void Remove(SocialChallenge challenge)
    {
        _dbContext.SocialChallenges.Remove(challenge);
    }
}
