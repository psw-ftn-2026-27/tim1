using Exploration.Application.TourPreferences;
using Exploration.Application.Tours;

namespace Exploration.Application.TourPreferenceEditing;

public sealed class TourPreferenceEditingQueries
{
    private readonly ITourPreferenceReadRepository _tourPreferenceReadRepository;
    private readonly ITourReadRepository _tourReadRepository;

    public TourPreferenceEditingQueries(
        ITourPreferenceReadRepository tourPreferenceReadRepository,
        ITourReadRepository tourReadRepository)
    {
        _tourPreferenceReadRepository = tourPreferenceReadRepository;
        _tourReadRepository = tourReadRepository;
    }

    public Task<TourPreferenceDto?> GetMineAsync(Guid userId)
    {
        return _tourPreferenceReadRepository.GetByUserIdAsync(userId);
    }

    public Task<List<string>> GetAvailableTagsAsync()
    {
        return _tourReadRepository.GetPublishedTagsAsync();
    }
}
