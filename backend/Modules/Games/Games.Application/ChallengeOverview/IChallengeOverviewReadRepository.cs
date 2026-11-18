namespace Games.Application.ChallengeOverview;

public interface IChallengeOverviewReadRepository
{
    Task<ChallengeOverviewDto> GetAllAsync(string? name, string? type, string? status, int page, int pageSize);
}
