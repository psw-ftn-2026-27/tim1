using Games.Domain.LocationChallengeAttempts;

namespace Games.Application.LocationChallengeAttempts;

public interface ILocationChallengeAttemptRepository
{
    Task<LocationChallengeAttempt?> GetAsync(Guid challengeId, Guid userId);
    void Add(LocationChallengeAttempt attempt);
}
