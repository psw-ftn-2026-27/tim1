using Shared.Domain;

namespace Games.Application.ChallengeOverview;

public sealed record ChallengeOverviewItemDto(
    Guid Id,
    string Name,
    string Type,
    string Status,
    DateTime? CreatedAt);

public sealed record ChallengeOverviewDto(PageResult<ChallengeOverviewItemDto> Page, bool HasChallenges);
