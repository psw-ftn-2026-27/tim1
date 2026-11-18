using Games.Application.LocationChallengeManagement;
using Shared.Domain;

namespace Games.Application.LocationChallenges;

public interface ILocationChallengeReadRepository
{
    Task<PageResult<LocationChallengeSummaryDto>> GetAllAsync(int page, int pageSize);
    Task<LocationChallengeDto?> GetByIdAsync(Guid id);
}
