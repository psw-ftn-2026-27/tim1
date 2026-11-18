using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed class Tour : AggregateRoot
{
    private const int MinimumDescriptionLengthForPublishing = 100;
    private const int MinimumKeypointsForPublishing = 2;

    private readonly List<TransportTime> _transportTimes = [];
    private readonly List<Keypoint> _keypoints = [];

    public Guid AuthorId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public TourDifficulty Difficulty { get; private set; }
    public List<string> Tags { get; private set; }
    public TourStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public GroupSize GroupSize { get; private set; }
    public IReadOnlyList<TransportTime> TransportTimes => _transportTimes;
    public IReadOnlyList<Keypoint> Keypoints => _keypoints;
    public double Length { get; private set; }

    private Tour()
    {
        Name = null!;
        Description = null!;
        Tags = null!;
        GroupSize = null!;
    }

    public Tour(Guid authorId, string name, string description, TourDifficulty difficulty, List<string> tags, GroupSize groupSize)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A tour requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A tour requires a description.");
        }
        if (tags is null || tags.Count == 0)
        {
            throw new DomainException("A tour requires at least one tag.");
        }

        AuthorId = authorId;
        Name = name;
        Description = description;
        Difficulty = difficulty;
        Tags = tags;
        GroupSize = groupSize;
        Status = TourStatus.Draft;
    }

    public void AddTransportTime(TransportMode transport, int minutes)
    {
        if (Status == TourStatus.Archived)
        {
            throw new DomainException("An archived tour cannot get a transport time.");
        }
        if (_transportTimes.Any(time => time.Transport == transport))
        {
            throw new DomainException($"The tour already has a time for {transport}.");
        }

        _transportTimes.Add(new TransportTime(transport, minutes));
    }

    public void AddKeypoint(string name, string description, Guid imageId, string? secret, bool publiclyVisible, Coordinate location)
    {
        EnsureDraft();

        _keypoints.Add(new Keypoint(name, description, imageId, secret, publiclyVisible, location));
    }

    // Returns the id of the image the keypoint no longer uses, or null when it keeps its image.
    public Guid? UpdateKeypoint(Guid keypointId, string name, string description, Guid imageId, string? secret, Coordinate location)
    {
        EnsureDraft();

        var keypoint = FindKeypoint(keypointId);
        var previousImageId = keypoint.ImageId;
        keypoint.Update(name, description, imageId, secret, location);

        return previousImageId == imageId ? null : previousImageId;
    }

    // Returns the id of the image the removed keypoint used.
    public Guid RemoveKeypoint(Guid keypointId)
    {
        EnsureDraft();

        var keypoint = FindKeypoint(keypointId);
        _keypoints.Remove(keypoint);

        return keypoint.ImageId;
    }

    // The length is measured on the map by the client; the domain only checks that it is a sensible number.
    public void SetLength(double lengthKm)
    {
        EnsureDraft();
        if (double.IsNaN(lengthKm) || double.IsInfinity(lengthKm) || lengthKm < 0)
        {
            throw new DomainException("A tour length cannot be negative.");
        }

        Length = lengthKm;
    }

    public void SetKeypointVisibility(Guid keypointId, bool publiclyVisible)
    {
        FindKeypoint(keypointId).SetPubliclyVisible(publiclyVisible);
    }

    public void Publish(decimal price)
    {
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("Only a draft tour can be published.");
        }
        if (price < 0)
        {
            throw new DomainException("A tour price cannot be negative.");
        }
        
        if (Description.Length < MinimumDescriptionLengthForPublishing)
        {
            throw new DomainException($"A tour can be published only with a description of at least {MinimumDescriptionLengthForPublishing} characters.");
        }
        if (_transportTimes.Count == 0)
        {
            throw new DomainException("A tour can be published only with at least one transport time.");
        }
        if (_keypoints.Count < MinimumKeypointsForPublishing)
        {
            throw new DomainException($"A tour can be published only with at least {MinimumKeypointsForPublishing} keypoints.");
        }

        Status = TourStatus.Published;
        PublishedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (Status != TourStatus.Published)
        {
            throw new DomainException("Only a published tour can be archived.");
        }

        Status = TourStatus.Archived;
        ArchivedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        if (Status != TourStatus.Archived)
        {
            throw new DomainException("Only an archived tour can be reactivated.");
        }

        Status = TourStatus.Published;
        ArchivedAt = null;
    }

    private void EnsureDraft()
    {
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("Keypoints can be changed only while the tour is a draft.");
        }
    }

    private Keypoint FindKeypoint(Guid keypointId)
    {
        var keypoint = _keypoints.FirstOrDefault(candidate => candidate.Id == keypointId);
        if (keypoint is null)
        {
            throw new NotFoundException($"Keypoint {keypointId} does not exist.");
        }
        return keypoint;
    }
}
