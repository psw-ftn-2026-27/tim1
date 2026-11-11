using Shared.Domain.Exceptions;

namespace Social.Domain.Clubs;

public static class ClubNameUniqueness
{
    public static void EnsureNameIsFree(Club club, Club? clubWithSameName)
    {
        if (clubWithSameName is not null
            && clubWithSameName.Id != club.Id
            && Normalize(clubWithSameName.Name) == Normalize(club.Name))
        {
            throw new DomainException($"Club name '{club.Name}' is already taken.");
        }
    }

    public static string Normalize(string name)
    {
        return name.Trim().ToLowerInvariant();
    }
}
