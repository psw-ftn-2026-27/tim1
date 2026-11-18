using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed class LocationChallenge : AggregateRoot
{
    private readonly List<HiddenPoint> _hiddenPoints = [];

    public string Name { get; private set; }
    public Location StartLocation { get; private set; }
    public int ExperiencePoints { get; private set; }
    public LocationChallengeStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<HiddenPoint> HiddenPoints => _hiddenPoints.AsReadOnly();

    private LocationChallenge()
    {
        Name = null!;
        StartLocation = null!;
    }

    public LocationChallenge(string name, Location startLocation, int experiencePoints, IReadOnlyList<HiddenPoint> hiddenPoints)
        : base(Guid.NewGuid())
    {
        Name = null!;
        StartLocation = null!;
        SetDetails(name, startLocation, experiencePoints, hiddenPoints);
        Status = LocationChallengeStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, Location startLocation, int experiencePoints, IReadOnlyList<HiddenPoint> hiddenPoints)
    {
        if (Status != LocationChallengeStatus.Draft)
        {
            throw new DomainException("Only a draft location challenge can be changed.");
        }

        SetDetails(name, startLocation, experiencePoints, hiddenPoints);
    }

    public void Publish()
    {
        if (Status != LocationChallengeStatus.Draft)
        {
            throw new DomainException("Only a draft location challenge can be published.");
        }

        Status = LocationChallengeStatus.Published;
    }

    public void Retire()
    {
        if (Status != LocationChallengeStatus.Published)
        {
            throw new DomainException("Only a published location challenge can be retired.");
        }

        Status = LocationChallengeStatus.Retired;
    }

    public void Archive()
    {
        if (Status != LocationChallengeStatus.Retired)
        {
            throw new DomainException("Only a retired location challenge can be archived.");
        }

        Status = LocationChallengeStatus.Archived;
    }

    public void Delete()
    {
        if (Status is not (LocationChallengeStatus.Draft or LocationChallengeStatus.Archived))
        {
            throw new DomainException("Only a draft or archived location challenge can be deleted.");
        }
    }

    private void SetDetails(string name, Location startLocation, int experiencePoints, IReadOnlyList<HiddenPoint> hiddenPoints)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A location challenge requires a name.");
        }
        if (experiencePoints <= 0)
        {
            throw new DomainException("A location challenge requires experience points greater than zero.");
        }
        if (hiddenPoints.Count == 0)
        {
            throw new DomainException("A location challenge requires at least one hidden point.");
        }

        Name = name;
        StartLocation = startLocation;
        ExperiencePoints = experiencePoints;
        _hiddenPoints.Clear();
        _hiddenPoints.AddRange(hiddenPoints);
    }
}
