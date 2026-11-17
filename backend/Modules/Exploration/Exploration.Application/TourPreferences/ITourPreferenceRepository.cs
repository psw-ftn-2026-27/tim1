using Exploration.Domain.TourPreferences;

namespace Exploration.Application.TourPreferences;

public interface ITourPreferenceRepository
{
    Task<TourPreference?> GetByUserIdAsync(Guid userId);
    void Add(TourPreference preference);
    void Remove(TourPreference preference);
}
