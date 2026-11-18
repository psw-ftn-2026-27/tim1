using Games.Domain.LocationChallenges;

namespace Games.Application.LocationChallenges;

public interface ILocationChallengeRepository
{
    Task<LocationChallenge?> GetByIdAsync(Guid id);
    void Add(LocationChallenge challenge);
    void Remove(LocationChallenge challenge);
}
