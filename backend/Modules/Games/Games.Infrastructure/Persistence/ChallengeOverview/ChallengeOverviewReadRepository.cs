using Games.Application.ChallengeOverview;
using Games.Domain.Quizzes;
using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Games.Infrastructure.Persistence.ChallengeOverview;

internal sealed class ChallengeOverviewReadRepository : IChallengeOverviewReadRepository
{
    private readonly GamesDbContext _dbContext;

    public ChallengeOverviewReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ChallengeOverviewDto> GetAllAsync(string? name, string? type, string? status, int page, int pageSize)
    {
        var quizzes = _dbContext.Quizzes.Select(quiz => new
        {
            quiz.Id,
            quiz.Name,
            Type = nameof(Quiz),
            Status = quiz.Status.ToString(),
            CreatedAt = (DateTime?)quiz.CreatedAt
        });
        var socialChallenges = _dbContext.SocialChallenges.Select(challenge => new
        {
            challenge.Id,
            challenge.Name,
            Type = nameof(SocialChallenge),
            Status = challenge.Status.ToString(),
            challenge.CreatedAt
        });

        var challenges = quizzes.Concat(socialChallenges);
        var hasChallenges = await challenges.AnyAsync();

        if (!string.IsNullOrEmpty(name))
        {
            challenges = challenges.Where(challenge => challenge.Name.ToLower().Contains(name));
        }

        if (type is not null)
        {
            challenges = challenges.Where(challenge => challenge.Type == type);
        }

        if (status is not null)
        {
            challenges = challenges.Where(challenge => challenge.Status == status);
        }

        var totalCount = await challenges.CountAsync();
        var offset = (long)(page - 1) * pageSize;
        var items = offset >= totalCount
            ? []
            : await challenges
                .OrderBy(challenge => challenge.Name)
                .ThenBy(challenge => challenge.Type)
                .ThenBy(challenge => challenge.Id)
                .Skip((int)offset)
                .Take(pageSize)
                .Select(challenge => new ChallengeOverviewItemDto(
                    challenge.Id, challenge.Name, challenge.Type, challenge.Status, challenge.CreatedAt))
                .ToListAsync();

        return new ChallengeOverviewDto(new PageResult<ChallengeOverviewItemDto>(items, totalCount), hasChallenges);
    }
}
