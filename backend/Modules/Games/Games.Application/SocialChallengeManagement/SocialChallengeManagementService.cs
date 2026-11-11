using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeManagement;

public sealed class SocialChallengeManagementService
{
    private readonly ISocialChallengeRepository _socialChallengeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SocialChallengeManagementService(ISocialChallengeRepository socialChallengeRepository, IUnitOfWork unitOfWork)
    {
        _socialChallengeRepository = socialChallengeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(CreateSocialChallengeDto dto)
    {
        var challenge = new SocialChallenge(
            dto.Name,
            dto.ActiveFrom,
            dto.ActiveUntil,
            dto.RequiredParticipants,
            dto.RadiusMeters,
            dto.ExperiencePoints,
            dto.CompletionMessage,
            dto.Latitude,
            dto.Longitude);

        _socialChallengeRepository.Add(challenge);
        await _unitOfWork.SaveChangesAsync();

        return challenge.Id;
    }

    public async Task UpdateAsync(Guid challengeId, UpdateSocialChallengeDto dto)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Update(
            dto.Name,
            dto.ActiveFrom,
            dto.ActiveUntil,
            dto.RequiredParticipants,
            dto.RadiusMeters,
            dto.ExperiencePoints,
            dto.CompletionMessage,
            dto.Latitude,
            dto.Longitude);
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

        _socialChallengeRepository.Remove(challenge);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<SocialChallenge> GetChallengeAsync(Guid challengeId)
    {
        var challenge = await _socialChallengeRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException($"Social challenge {challengeId} does not exist.");
        }

        return challenge;
    }
}
