using Exploration.Contracts;
using Games.Application.LocationChallengeAttempts;
using Games.Application.LocationChallenges;
using Games.Domain.LocationChallengeAttempts;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeSolving;

public sealed class LocationChallengeSolvingService
{
    private readonly ILocationChallengeRepository _locationChallengeRepository;
    private readonly ILocationChallengeAttemptRepository _attemptRepository;
    private readonly LocationChallengeActivation _activation;
    private readonly IExplorationApi _explorationApi;
    private readonly IUnitOfWork _unitOfWork;

    public LocationChallengeSolvingService(
        ILocationChallengeRepository locationChallengeRepository,
        ILocationChallengeAttemptRepository attemptRepository,
        LocationChallengeActivation activation,
        IExplorationApi explorationApi,
        IUnitOfWork unitOfWork)
    {
        _locationChallengeRepository = locationChallengeRepository;
        _attemptRepository = attemptRepository;
        _activation = activation;
        _explorationApi = explorationApi;
        _unitOfWork = unitOfWork;
    }

    public async Task ActivateAsync(Guid challengeId, Guid userId)
    {
        var challenge = await _locationChallengeRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException($"Location challenge {challengeId} does not exist.");
        }

        var position = await _explorationApi.GetCurrentPositionAsync(userId);
        if (position is null)
        {
            throw new DomainException("Set your position before activating a location challenge.");
        }

        var existingAttempt = await _attemptRepository.GetAsync(challengeId, userId);
        var extraMeters = Random.Shared.NextDouble() * SearchCircle.MaxExtraMeters;
        var attempt = _activation.Activate(
            challenge,
            existingAttempt,
            userId,
            new Location(position.Latitude, position.Longitude),
            extraMeters);

        if (existingAttempt is null)
        {
            _attemptRepository.Add(attempt);
        }
        await _unitOfWork.SaveChangesAsync();
    }
}
