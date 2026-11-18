using Games.Application.LocationChallengeManagement;
using Games.Application.LocationChallengeSolving;
using Shared.Domain;

namespace Games.Application.LocationChallenges;

public interface ILocationChallengeReadRepository
{
    Task<PageResult<LocationChallengeSummaryDto>> GetAllAsync(int page, int pageSize);
    Task<LocationChallengeDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<PublishedLocationChallengeDto>> GetPublishedAsync();
}
