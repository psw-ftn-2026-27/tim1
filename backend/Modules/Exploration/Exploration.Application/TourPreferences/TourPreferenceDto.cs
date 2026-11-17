using Exploration.Domain.Tours;

namespace Exploration.Application.TourPreferences;

public sealed record TourPreferenceDto(
    List<TourDifficulty> Difficulties,
    TransportMode? Transport,
    List<string> Tags,
    bool OpensSearch);
