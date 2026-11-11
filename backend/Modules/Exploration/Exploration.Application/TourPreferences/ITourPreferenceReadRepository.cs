using Exploration.Application.TourPreferenceEditing;

namespace Exploration.Application.TourPreferences;

public interface ITourPreferenceReadRepository
{
    Task<TourPreferenceDto?> GetByUserIdAsync(Guid userId);
}
