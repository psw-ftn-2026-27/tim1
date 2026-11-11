using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record TourSearchDto
{
    public string? Title { get; init; }
    public List<TourDifficulty>? Difficulties { get; init; }
    public List<TransportMode>? Transports { get; init; }
    public List<string>? Tags { get; init; }
    public TourSortOrder Sort { get; init; } = TourSortOrder.Newest;
}
