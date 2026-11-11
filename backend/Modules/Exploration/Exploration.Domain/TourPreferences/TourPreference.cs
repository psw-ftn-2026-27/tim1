using Exploration.Domain.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourPreferences;

public sealed class TourPreference : AggregateRoot
{
    public Guid UserId { get; private set; }
    public List<TourDifficulty> Difficulties { get; private set; }
    public TransportMode? Transport { get; private set; }
    public List<string> Tags { get; private set; }
    public bool OpensSearch { get; private set; }

    private TourPreference()
    {
        Difficulties = null!;
        Tags = null!;
    }

    public TourPreference(Guid userId, List<TourDifficulty> difficulties, TransportMode? transport, List<string> tags, bool opensSearch)
        : base(Guid.NewGuid())
    {
        UserId = userId;
        Difficulties = null!;
        Tags = null!;
        Set(difficulties, transport, tags, opensSearch);
    }

    public void Change(List<TourDifficulty> difficulties, TransportMode? transport, List<string> tags, bool opensSearch)
    {
        Set(difficulties, transport, tags, opensSearch);
    }

    private void Set(List<TourDifficulty> difficulties, TransportMode? transport, List<string> tags, bool opensSearch)
    {
        var distinctDifficulties = (difficulties ?? []).Distinct().ToList();
        var distinctTags = (tags ?? []).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (distinctDifficulties.Count == 0 && transport is null && distinctTags.Count == 0)
        {
            throw new DomainException("Tour preferences require a difficulty, a transport, or a tag.");
        }

        Difficulties = distinctDifficulties;
        Transport = transport;
        Tags = distinctTags;
        OpensSearch = opensSearch;
    }
}
