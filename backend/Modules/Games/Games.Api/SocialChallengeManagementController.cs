using Games.Application.SocialChallengeManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;

namespace Games.Api;

[ApiController]
[Route("api/games/social-challenges")]
[Authorize(Roles = "administrator")]
public sealed class SocialChallengeManagementController : ControllerBase
{
    private readonly SocialChallengeManagementService _managementService;
    private readonly SocialChallengeManagementQueries _managementQueries;

    public SocialChallengeManagementController(
        SocialChallengeManagementService managementService,
        SocialChallengeManagementQueries managementQueries)
    {
        _managementService = managementService;
        _managementQueries = managementQueries;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<SocialChallengeDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _managementQueries.GetAllAsync(page, pageSize);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SocialChallengeDto>> GetById(Guid id)
    {
        return await _managementQueries.GetByIdAsync(id);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateSocialChallengeDto dto)
    {
        return await _managementService.CreateAsync(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpdateSocialChallengeDto dto)
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
