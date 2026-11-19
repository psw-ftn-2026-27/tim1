using Games.Domain.ChallengeCompletions;

namespace Games.Application.ChallengeCompletions;

public interface IChallengeCompletionRepository
{
    Task<ChallengeCompletion?> GetByPlayerAndChallengeAsync(Guid playerId, Guid challengeId);
    void Add(ChallengeCompletion completion);
}
