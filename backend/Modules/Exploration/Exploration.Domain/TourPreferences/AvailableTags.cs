using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourPreferences;

public static class AvailableTags
{
    public static void EnsureSelectable(List<string> chosenTags, List<string> savedTags, IReadOnlyList<Tour> tours)
    {
        var availableTags = tours
            .Where(tour => tour.Status == TourStatus.Published)
            .SelectMany(tour => tour.Tags)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var alreadySaved = savedTags.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var unavailableTag = (chosenTags ?? [])
            .Where(tag => !alreadySaved.Contains(tag))
            .FirstOrDefault(tag => !availableTags.Contains(tag));
        if (unavailableTag is not null)
        {
            throw new DomainException($"The tag {unavailableTag} is not on any published tour.");
        }
    }
}
