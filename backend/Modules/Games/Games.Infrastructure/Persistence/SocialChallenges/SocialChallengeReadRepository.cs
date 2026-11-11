using Games.Application.SocialChallengeManagement;
using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal sealed class SocialChallengeReadRepository : ISocialChallengeReadRepository
{
    private readonly GamesDbContext _dbContext;

    public SocialChallengeReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SocialChallengeDto?> GetByIdAsync(Guid id)
    {
        return ProjectToDtos(_dbContext.SocialChallenges.Where(challenge => challenge.Id == id)).FirstOrDefaultAsync();
    }

    public async Task<PageResult<SocialChallengeDto>> GetAllAsync(int page, int pageSize)
    {
        // Names are not unique, so the ID breaks ties and keeps every page stable.
        var challenges = _dbContext.SocialChallenges
            .OrderBy(challenge => challenge.Name)
            .ThenBy(challenge => challenge.Id);

        var totalCount = await challenges.CountAsync();
        var items = await ProjectToDtos(challenges)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<SocialChallengeDto>(items, totalCount);
    }

    private static IQueryable<SocialChallengeDto> ProjectToDtos(IQueryable<SocialChallenge> challenges)
    {
        return challenges.Select(challenge => new SocialChallengeDto(
            challenge.Id,
            challenge.Name,
            challenge.ActiveFrom,
            challenge.ActiveUntil,
            challenge.RequiredParticipants,
            challenge.RadiusMeters,
            challenge.ExperiencePoints,
            challenge.CompletionMessage,
            challenge.Latitude,
            challenge.Longitude,
            challenge.Status));
    }
}
