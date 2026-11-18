using Games.Application.LocationChallengeManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;

namespace Games.Api;

[ApiController]
[Route("api/games/location-challenges")]
[Authorize(Roles = "administrator")]
public sealed class LocationChallengeManagementController : ControllerBase
{
    private readonly LocationChallengeManagementService _managementService;
    private readonly LocationChallengeManagementQueries _managementQueries;

    public LocationChallengeManagementController(
        LocationChallengeManagementService managementService,
        LocationChallengeManagementQueries managementQueries)
    {
        _managementService = managementService;
        _managementQueries = managementQueries;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<LocationChallengeSummaryDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return await _managementQueries.GetAllAsync(page, pageSize);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LocationChallengeDto>> GetById(Guid id)
    {
        return await _managementQueries.GetByIdAsync(id);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(SaveLocationChallengeDto dto)
    {
        return await _managementService.CreateAsync(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, SaveLocationChallengeDto dto)
    {
        await _managementService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult> Publish(Guid id)
    {
        await _managementService.PublishAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/retire")]
    public async Task<ActionResult> Retire(Guid id)
    {
        await _managementService.RetireAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult> Archive(Guid id)
    {
        await _managementService.ArchiveAsync(id);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _managementService.DeleteAsync(id);
        return NoContent();
    }
}
