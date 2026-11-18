using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.UserPositions;

public sealed class UserPosition : AggregateRoot
{
    public Guid UserId { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private UserPosition() { }

    public UserPosition(Guid userId, double? latitude, double? longitude) : base(Guid.NewGuid())
    {
        UserId = userId;
        ChangePosition(latitude, longitude);
    }

    public void ChangePosition(double? latitude, double? longitude)
    {
        if (latitude is not (>= -90 and <= 90))
        {
            throw new DomainException("Latitude must be between -90 and 90.");
        }
        if (longitude is not (>= -180 and <= 180))
        {
            throw new DomainException("Longitude must be between -180 and 180.");
        }

        Latitude = latitude.Value;
        Longitude = longitude.Value;
        UpdatedAt = DateTime.UtcNow;
    }
}
