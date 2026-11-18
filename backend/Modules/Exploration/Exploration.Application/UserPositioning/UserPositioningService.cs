using Exploration.Application.UserPositions;
using Exploration.Domain.UserPositions;

namespace Exploration.Application.UserPositioning;

public sealed class UserPositioningService
{
    private readonly IUserPositionRepository _positionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserPositioningService(IUserPositionRepository positionRepository, IUnitOfWork unitOfWork)
    {
        _positionRepository = positionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SaveCurrentPositionAsync(Guid userId, SaveCurrentPositionDto dto)
    {
        var position = await _positionRepository.GetByUserIdAsync(userId);
        if (position is null)
        {
            _positionRepository.Add(new UserPosition(userId, dto.Latitude, dto.Longitude));
        }
        else
        {
            position.ChangePosition(dto.Latitude, dto.Longitude);
            _positionRepository.UpdateCoordinates(position);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
