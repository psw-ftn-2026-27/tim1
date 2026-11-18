using Exploration.Application.UserPositioning;
using Exploration.Contracts;

namespace Exploration.Application;

public sealed class ExplorationApi : IExplorationApi
{
    private readonly UserPositioningQueries _positioningQueries;

    public ExplorationApi(UserPositioningQueries positioningQueries)
    {
        _positioningQueries = positioningQueries;
    }

    public async Task<UserPositionDto?> GetCurrentPositionAsync(Guid userId)
    {
        var position = await _positioningQueries.GetMyCurrentPositionAsync(userId);
        return position is null ? null : new UserPositionDto(position.Latitude, position.Longitude, position.UpdatedAt);
    }
}
