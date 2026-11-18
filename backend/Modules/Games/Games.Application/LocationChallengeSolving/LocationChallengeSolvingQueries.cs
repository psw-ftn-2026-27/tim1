using Exploration.Contracts;
using Games.Application.Images;
using Games.Application.LocationChallengeAttempts;
using Games.Application.LocationChallenges;
using Games.Domain.LocationChallengeAttempts;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeSolving;

public sealed class LocationChallengeSolvingQueries
{
    private readonly ILocationChallengeReadRepository _locationChallengeReadRepository;
    private readonly ILocationChallengeAttemptReadRepository _attemptReadRepository;
    private readonly IImageReadRepository _imageReadRepository;
    private readonly IImageStorage _imageStorage;
    private readonly IExplorationApi _explorationApi;

    public LocationChallengeSolvingQueries(
        ILocationChallengeReadRepository locationChallengeReadRepository,
        ILocationChallengeAttemptReadRepository attemptReadRepository,
        IImageReadRepository imageReadRepository,
        IImageStorage imageStorage,
        IExplorationApi explorationApi)
    {
        _locationChallengeReadRepository = locationChallengeReadRepository;
        _attemptReadRepository = attemptReadRepository;
        _imageReadRepository = imageReadRepository;
        _imageStorage = imageStorage;
        _explorationApi = explorationApi;
    }

    // Challenges the explorer has solved are left out. The list is small, because
    // administrators make every challenge, so it is sorted by distance in memory and not paged.
    public async Task<NearbyLocationChallengesDto> GetNearbyAsync(Guid userId)
    {
        var position = await _explorationApi.GetCurrentPositionAsync(userId);
        if (position is null)
        {
            return new NearbyLocationChallengesDto(null, []);
        }

        var userLocation = new Location(position.Latitude, position.Longitude);
        var published = await _locationChallengeReadRepository.GetPublishedAsync();
        var solvedChallengeIds = await _attemptReadRepository.GetSolvedChallengeIdsAsync(userId);
        var startedChallengeIds = await _attemptReadRepository.GetStartedChallengeIdsAsync(userId);
        var challenges = published
            .Where(challenge => !solvedChallengeIds.Contains(challenge.Id))
            .Select(challenge => ToNearby(challenge, userLocation, startedChallengeIds.Contains(challenge.Id)))
            .OrderBy(challenge => challenge.DistanceMeters)
            .ToList();

        return new NearbyLocationChallengesDto(new LocationDto(position.Latitude, position.Longitude), challenges);
    }

    public async Task<LocationChallengeAttemptDto> GetAttemptAsync(Guid challengeId, Guid userId)
    {
        var attempt = await _attemptReadRepository.GetAsync(challengeId, userId);
        if (attempt is null)
        {
            throw new NotFoundException($"You have not activated location challenge {challengeId}.");
        }

        return attempt;
    }

    public async Task<ImageContentDto> GetAttemptImageAsync(Guid challengeId, Guid imageId, Guid userId)
    {
        var attempt = await GetAttemptAsync(challengeId, userId);
        if (!attempt.Photos.Any(photo => photo.ImageId == imageId))
        {
            throw new NotFoundException($"Image {imageId} is not a photo of location challenge {challengeId}.");
        }

        var file = await _imageReadRepository.GetByIdAsync(imageId);
        if (file is null)
        {
            throw new NotFoundException($"Image {imageId} does not exist.");
        }

        var content = await _imageStorage.ReadAsync(file.StoredFileName);
        if (content is null)
        {
            throw new NotFoundException($"Image {imageId} does not exist.");
        }

        return content;
    }

    private static NearbyLocationChallengeDto ToNearby(
        PublishedLocationChallengeDto challenge,
        Location userLocation,
        bool isStarted)
    {
        var startLocation = new Location(challenge.StartLocation.Latitude, challenge.StartLocation.Longitude);
        var distance = userLocation.DistanceTo(startLocation);

        return new NearbyLocationChallengeDto(
            challenge.Id,
            challenge.Name,
            challenge.ExperiencePoints,
            distance,
            distance <= LocationChallengeActivation.MaxDistanceMeters,
            isStarted);
    }
}
