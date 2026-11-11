using Games.Application.SocialChallenges;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeManagement;

public sealed class SocialChallengeManagementQueries
{
    private readonly ISocialChallengeReadRepository _socialChallengeReadRepository;

    public SocialChallengeManagementQueries(ISocialChallengeReadRepository socialChallengeReadRepository)
    {
        _socialChallengeReadRepository = socialChallengeReadRepository;
    }

    public Task<PageResult<SocialChallengeDto>> GetAllAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _socialChallengeReadRepository.GetAllAsync(page, pageSize);
    }

    public async Task<SocialChallengeDto> GetByIdAsync(Guid id)
    {
        var challenge = await _socialChallengeReadRepository.GetByIdAsync(id);
        if (challenge is null)
        {
            throw new NotFoundException($"Social challenge {id} does not exist.");
        }

        return challenge;
    }
}
