using Games.Application.LocationChallengeManagement;
using Games.Application.LocationChallenges;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal sealed class LocationChallengeReadRepository : ILocationChallengeReadRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<LocationChallengeSummaryDto>> GetAllAsync(int page, int pageSize)
    {
        // Names are not unique, so the ID breaks ties and keeps every page stable.
        var challenges = _dbContext.LocationChallenges
            .OrderBy(challenge => challenge.Name)
            .ThenBy(challenge => challenge.Id);

        var totalCount = await challenges.CountAsync();
        var items = await challenges
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(challenge => new LocationChallengeSummaryDto(
                challenge.Id,
                challenge.Name,
                challenge.HiddenPoints.Count,
                challenge.ExperiencePoints,
                challenge.Status))
            .ToListAsync();

        return new PageResult<LocationChallengeSummaryDto>(items, totalCount);
    }

    public Task<LocationChallengeDto?> GetByIdAsync(Guid id)
    {
        return _dbContext.LocationChallenges
            .AsNoTracking()
            .Where(challenge => challenge.Id == id)
            .Select(challenge => new LocationChallengeDto(
                challenge.Id,
                challenge.Name,
                new LocationDto(challenge.StartLocation.Latitude, challenge.StartLocation.Longitude),
                challenge.ExperiencePoints,
                challenge.Status,
                challenge.HiddenPoints
                    .Select(point => new HiddenPointDto(
                        point.Id,
                        new LocationDto(point.Location.Latitude, point.Location.Longitude),
                        point.ImageId))
                    .ToList()))
            .SingleOrDefaultAsync();
    }
}
