using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed class SocialChallenge : AggregateRoot
{
    private const int MinimumRequiredParticipants = 2;

    public string Name { get; private set; }
    public TimeOnly ActiveFrom { get; private set; }
    public TimeOnly ActiveUntil { get; private set; }
    public int RequiredParticipants { get; private set; }
    public int RadiusMeters { get; private set; }
    public int ExperiencePoints { get; private set; }
    public string CompletionMessage { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public SocialChallengeStatus Status { get; private set; }

    private SocialChallenge()
    {
        Name = null!;
        CompletionMessage = null!;
    }

    public SocialChallenge(
        string name,
        TimeOnly activeFrom,
        TimeOnly activeUntil,
        int requiredParticipants,
        int radiusMeters,
        int experiencePoints,
        string completionMessage,
        double? latitude,
        double? longitude)
        : base(Guid.NewGuid())
    {
        Name = null!;
        CompletionMessage = null!;
        SetDetails(name, activeFrom, activeUntil, requiredParticipants, radiusMeters, experiencePoints, completionMessage, latitude, longitude);
        Status = SocialChallengeStatus.Draft;
    }

    public void Update(
        string name,
        TimeOnly activeFrom,
        TimeOnly activeUntil,
        int requiredParticipants,
        int radiusMeters,
        int experiencePoints,
        string completionMessage,
        double? latitude,
        double? longitude)
    {
        if (Status != SocialChallengeStatus.Draft)
        {
            throw new DomainException("Only a draft social challenge can be changed.");
        }

        SetDetails(name, activeFrom, activeUntil, requiredParticipants, radiusMeters, experiencePoints, completionMessage, latitude, longitude);
    }

    public void Publish()
    {
        if (Status != SocialChallengeStatus.Draft)
        {
            throw new DomainException("Only a draft social challenge can be published.");
        }

        Status = SocialChallengeStatus.Published;
    }

    public void Retire()
    {
        if (Status != SocialChallengeStatus.Published)
        {
            throw new DomainException("Only a published social challenge can be retired.");
        }

        Status = SocialChallengeStatus.Retired;
    }

    public void Archive()
    {
        if (Status != SocialChallengeStatus.Retired)
        {
            throw new DomainException("Only a retired social challenge can be archived.");
        }

        Status = SocialChallengeStatus.Archived;
    }

    public void Delete()
    {
        if (Status != SocialChallengeStatus.Draft && Status != SocialChallengeStatus.Archived)
        {
            throw new DomainException("Only a draft or archived social challenge can be deleted.");
        }
    }

    private void SetDetails(
        string name,
        TimeOnly activeFrom,
        TimeOnly activeUntil,
        int requiredParticipants,
        int radiusMeters,
        int experiencePoints,
        string completionMessage,
        double? latitude,
        double? longitude)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A social challenge requires a name.");
        }
        if (string.IsNullOrWhiteSpace(completionMessage))
        {
            throw new DomainException("A social challenge requires a completion message.");
        }
        if (activeFrom >= activeUntil)
        {
            throw new DomainException("A social challenge must start before it ends, within the same day.");
        }
        if (requiredParticipants < MinimumRequiredParticipants)
        {
            throw new DomainException($"A social challenge requires at least {MinimumRequiredParticipants} participants.");
        }
        if (radiusMeters <= 0)
        {
            throw new DomainException("A social challenge requires a radius greater than zero.");
        }
        if (experiencePoints <= 0)
        {
            throw new DomainException("A social challenge requires experience points greater than zero.");
        }
        if (latitude is null || longitude is null)
        {
            throw new DomainException("A social challenge requires a location.");
        }
        if (latitude < -90 || latitude > 90)
        {
            throw new DomainException("A social challenge requires a latitude between -90 and 90.");
        }
        if (longitude < -180 || longitude > 180)
        {
            throw new DomainException("A social challenge requires a longitude between -180 and 180.");
        }

        Name = name;
        ActiveFrom = activeFrom;
        ActiveUntil = activeUntil;
        RequiredParticipants = requiredParticipants;
        RadiusMeters = radiusMeters;
        ExperiencePoints = experiencePoints;
        CompletionMessage = completionMessage;
        Latitude = latitude.Value;
        Longitude = longitude.Value;
    }
}
