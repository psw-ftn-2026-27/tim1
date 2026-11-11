using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;
using Social.Application.ClubBrowsing;
using Social.Application.Clubs;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize]
public sealed class ClubBrowsingController : ControllerBase
{
    private readonly ClubBrowsingQueries _browsingQueries;

    public ClubBrowsingController(ClubBrowsingQueries browsingQueries)
    {
        _browsingQueries = browsingQueries;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<ClubDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _browsingQueries.GetAllAsync(page, pageSize);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClubDto>> GetById(Guid id)
    {
        return await _browsingQueries.GetByIdAsync(id);
    }
}
