using Exploration.Application.TourPreferenceEditing;
using Exploration.Application.TourPreferences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tour-preferences")]
[Authorize]
public sealed class TourPreferenceEditingController : ControllerBase
{
    private readonly TourPreferenceEditingService _editingService;
    private readonly TourPreferenceEditingQueries _editingQueries;

    public TourPreferenceEditingController(
        TourPreferenceEditingService editingService,
        TourPreferenceEditingQueries editingQueries)
    {
        _editingService = editingService;
        _editingQueries = editingQueries;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<TourPreferenceDto?>> GetMine()
    {
        return await _editingQueries.GetMineAsync(User.GetUserId());
    }

    [HttpGet("available-tags")]
    public async Task<ActionResult<List<string>>> GetAvailableTags()
    {
        return await _editingQueries.GetAvailableTagsAsync();
    }

    [HttpPut("mine")]
    public async Task<ActionResult> Save(TourPreferenceDto dto)
    {
        await _editingService.SaveAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete("mine")]
    public async Task<ActionResult> Delete()
    {
        await _editingService.DeleteAsync(User.GetUserId());
        return NoContent();
    }
}
