using Exploration.Application.UserPositions;

namespace Exploration.Application.UserPositioning;

public sealed class UserPositioningQueries
{
    private readonly IUserPositionReadRepository _positionReadRepository;

    public UserPositioningQueries(IUserPositionReadRepository positionReadRepository)
    {
        _positionReadRepository = positionReadRepository;
    }

    public Task<CurrentPositionDto?> GetMyCurrentPositionAsync(Guid userId)
    {
        return _positionReadRepository.GetByUserIdAsync(userId);
    }
}
