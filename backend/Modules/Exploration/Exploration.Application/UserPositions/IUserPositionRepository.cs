using Exploration.Domain.UserPositions;

namespace Exploration.Application.UserPositions;

public interface IUserPositionRepository
{
    Task<UserPosition?> GetByUserIdAsync(Guid userId);
    void Add(UserPosition position);
    void UpdateCoordinates(UserPosition position);
}
