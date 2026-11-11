using Exploration.Domain.Tours;

namespace Exploration.Application.TourPreferenceEditing;

public sealed record TourPreferenceDto(
    List<TourDifficulty> Difficulties,
    TransportMode? Transport,
    List<string> Tags,
    bool OpensSearch);
