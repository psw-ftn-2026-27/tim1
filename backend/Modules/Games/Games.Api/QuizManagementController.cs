using Games.Application.QuizManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;

namespace Games.Api;

[ApiController]
[Route("api/games/quizzes")]
[Authorize(Roles = "administrator")]
public sealed class QuizManagementController : ControllerBase
{
    private readonly QuizManagementService _managementService;
    private readonly QuizManagementQueries _managementQueries;

    public QuizManagementController(QuizManagementService managementService, QuizManagementQueries managementQueries)
    {
        _managementService = managementService;
        _managementQueries = managementQueries;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<QuizSummaryDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _managementQueries.GetAllAsync(page, pageSize);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuizDto>> GetById(Guid id)
    {
        return await _managementQueries.GetByIdAsync(id);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(SaveQuizDto dto)
    {
        return await _managementService.CreateAsync(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, SaveQuizDto dto)
    {
        await _managementService.UpdateAsync(id, dto);
        return NoContent();
    }
}
