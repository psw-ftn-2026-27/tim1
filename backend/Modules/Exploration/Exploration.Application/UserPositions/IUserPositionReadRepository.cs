using Exploration.Application.UserPositioning;

namespace Exploration.Application.UserPositions;

public interface IUserPositionReadRepository
{
    Task<CurrentPositionDto?> GetByUserIdAsync(Guid userId);
}
