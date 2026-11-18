namespace Games.Application.ChallengeOverview;

public sealed class ChallengeOverviewQueries
{
    private readonly IChallengeOverviewReadRepository _readRepository;

    public ChallengeOverviewQueries(IChallengeOverviewReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public Task<ChallengeOverviewDto> GetAllAsync(string? name, string? type, string? status, int page, int pageSize)
    {
        return _readRepository.GetAllAsync(
            name?.Trim().ToLowerInvariant(),
            type,
            status,
            Math.Max(page, 1),
            Math.Clamp(pageSize, 1, 100));
    }
}
