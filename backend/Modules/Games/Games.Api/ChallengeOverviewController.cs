using Games.Application.ChallengeOverview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/challenges")]
[Authorize(Roles = "administrator")]
public sealed class ChallengeOverviewController : ControllerBase
{
    private readonly ChallengeOverviewQueries _queries;

    public ChallengeOverviewController(ChallengeOverviewQueries queries)
    {
        _queries = queries;
    }

    [HttpGet]
    public Task<ChallengeOverviewDto> GetAll(
        [FromQuery] string? name = null,
        [FromQuery] string? type = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return _queries.GetAllAsync(name, type, status, page, pageSize);
    }
}
