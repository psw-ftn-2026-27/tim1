using Games.Application.LocationChallengeAttempts;
using Games.Application.LocationChallenges;
using Games.Application.LocationChallengeSolving;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallengeAttempts;

internal sealed class LocationChallengeAttemptReadRepository : ILocationChallengeAttemptReadRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeAttemptReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // The hidden points' locations are deliberately left out: the explorer sees only the photos.
    // The awarded experience points come from the challenge completion, which exists once the attempt is solved.
    public Task<LocationChallengeAttemptDto?> GetAsync(Guid challengeId, Guid userId)
    {
        return _dbContext.LocationChallengeAttempts
            .AsNoTracking()
            .Where(attempt => attempt.ChallengeId == challengeId && attempt.UserId == userId)
            .Join(
                _dbContext.LocationChallenges,
                attempt => attempt.ChallengeId,
                challenge => challenge.Id,
                (attempt, challenge) => new LocationChallengeAttemptDto(
                    attempt.Id,
                    challenge.Id,
                    challenge.Name,
                    new SearchCircleDto(
                        new LocationDto(attempt.Circle.Center.Latitude, attempt.Circle.Center.Longitude),
                        attempt.Circle.DiameterMeters),
                    challenge.HiddenPoints
                        .Select(point => new AttemptPhotoDto(point.Id, point.ImageId, attempt.DiscoveredPointIds.Contains(point.Id)))
                        .ToList(),
                    challenge.HiddenPoints.Count(point => !attempt.DiscoveredPointIds.Contains(point.Id)),
                    attempt.SolvedAt != null,
                    attempt.StartedAt,
                    attempt.SolvedAt,
                    _dbContext.ChallengeCompletions
                        .Where(completion => completion.PlayerId == attempt.UserId
                            && completion.ChallengeId == attempt.ChallengeId)
                        .Select(completion => (int?)completion.ExperiencePointsAwarded)
                        .SingleOrDefault()))
            .SingleOrDefaultAsync();
    }

    public async Task<IReadOnlyList<Guid>> GetSolvedChallengeIdsAsync(Guid userId)
    {
        return await _dbContext.LocationChallengeAttempts
            .AsNoTracking()
            .Where(attempt => attempt.UserId == userId && attempt.SolvedAt != null)
            .Select(attempt => attempt.ChallengeId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Guid>> GetStartedChallengeIdsAsync(Guid userId)
    {
        return await _dbContext.LocationChallengeAttempts
            .AsNoTracking()
            .Where(attempt => attempt.UserId == userId && attempt.SolvedAt == null)
            .Select(attempt => attempt.ChallengeId)
            .ToListAsync();
    }
}
