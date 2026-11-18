using Games.Application.LocationChallenges;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeManagement;

public sealed class LocationChallengeManagementService
{
    private readonly ILocationChallengeRepository _locationChallengeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LocationChallengeManagementService(
        ILocationChallengeRepository locationChallengeRepository,
        IUnitOfWork unitOfWork)
    {
        _locationChallengeRepository = locationChallengeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(SaveLocationChallengeDto dto)
    {
        var challenge = new LocationChallenge(
            dto.Name,
            ToLocation(dto.StartLocation),
            dto.ExperiencePoints,
            ToHiddenPoints(dto.HiddenPoints));

        _locationChallengeRepository.Add(challenge);
        await _unitOfWork.SaveChangesAsync();

        return challenge.Id;
    }

    public async Task UpdateAsync(Guid challengeId, SaveLocationChallengeDto dto)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Update(
            dto.Name,
            ToLocation(dto.StartLocation),
            dto.ExperiencePoints,
            ToHiddenPoints(dto.HiddenPoints));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PublishAsync(Guid challengeId)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Publish();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RetireAsync(Guid challengeId)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Retire();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ArchiveAsync(Guid challengeId)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Archive();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid challengeId)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Delete();

        _locationChallengeRepository.Remove(challenge);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<LocationChallenge> GetChallengeAsync(Guid challengeId)
    {
        var challenge = await _locationChallengeRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException($"Location challenge {challengeId} does not exist.");
        }

        return challenge;
    }

    private static Location ToLocation(LocationDto location)
    {
        return new Location(location.Latitude, location.Longitude);
    }

    private static HiddenPoint[] ToHiddenPoints(IReadOnlyList<HiddenPointInputDto> hiddenPoints)
    {
        return hiddenPoints
            .Select(point => new HiddenPoint(ToLocation(point.Location), point.ImageId))
            .ToArray();
    }
}
