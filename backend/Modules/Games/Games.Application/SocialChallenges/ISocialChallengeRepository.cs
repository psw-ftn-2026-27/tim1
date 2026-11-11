using Games.Domain.SocialChallenges;

namespace Games.Application.SocialChallenges;

public interface ISocialChallengeRepository
{
    Task<SocialChallenge?> GetByIdAsync(Guid id);
    void Add(SocialChallenge challenge);
    void Remove(SocialChallenge challenge);
}
