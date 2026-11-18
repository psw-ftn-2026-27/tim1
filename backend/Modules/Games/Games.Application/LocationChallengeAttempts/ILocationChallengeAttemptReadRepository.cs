using Games.Application.LocationChallengeSolving;

namespace Games.Application.LocationChallengeAttempts;

public interface ILocationChallengeAttemptReadRepository
{
    Task<LocationChallengeAttemptDto?> GetAsync(Guid challengeId, Guid userId);
}
