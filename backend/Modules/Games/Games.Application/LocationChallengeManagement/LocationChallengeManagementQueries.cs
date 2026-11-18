using Games.Application.LocationChallenges;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeManagement;

public sealed class LocationChallengeManagementQueries
{
    private readonly ILocationChallengeReadRepository _locationChallengeReadRepository;

    public LocationChallengeManagementQueries(ILocationChallengeReadRepository locationChallengeReadRepository)
    {
        _locationChallengeReadRepository = locationChallengeReadRepository;
    }

    public Task<PageResult<LocationChallengeSummaryDto>> GetAllAsync(int page, int pageSize)
    {
        return _locationChallengeReadRepository.GetAllAsync(Math.Max(page, 1), Math.Clamp(pageSize, 1, 100));
    }

    public async Task<LocationChallengeDto> GetByIdAsync(Guid id)
    {
        var challenge = await _locationChallengeReadRepository.GetByIdAsync(id);
        if (challenge is null)
        {
            throw new NotFoundException($"Location challenge {id} does not exist.");
        }

        return challenge;
    }
}
