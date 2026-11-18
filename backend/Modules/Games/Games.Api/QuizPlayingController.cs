using Games.Application.QuizPlaying;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;

namespace Games.Api;

[ApiController]
[Route("api/games/play/quizzes")]
[Authorize(Roles = "explorer")]
public sealed class QuizPlayingController : ControllerBase
{
    private readonly QuizPlayingService _playingService;
    private readonly QuizPlayingQueries _playingQueries;

    public QuizPlayingController(QuizPlayingService playingService, QuizPlayingQueries playingQueries)
    {
        _playingService = playingService;
        _playingQueries = playingQueries;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<AvailableQuizDto>>> GetAvailable(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _playingQueries.GetAvailableAsync(User.GetUserId(), page, pageSize);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuizPlayStateDto>> GetState(Guid id)
    {
        return await _playingQueries.GetStateAsync(User.GetUserId(), id);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult> Activate(Guid id, ActivateQuizDto dto)
    {
        await _playingService.ActivateAsync(User.GetUserId(), id, dto);
        return NoContent();
    }

    [HttpPost("{id:guid}/answers")]
    public async Task<ActionResult> Answer(Guid id, AnswerQuestionDto dto)
    {
        await _playingService.AnswerAsync(User.GetUserId(), id, dto);
        return NoContent();
    }
}
