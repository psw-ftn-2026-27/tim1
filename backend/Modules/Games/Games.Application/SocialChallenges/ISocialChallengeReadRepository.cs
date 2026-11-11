using Games.Application.SocialChallengeManagement;
using Shared.Domain;

namespace Games.Application.SocialChallenges;

public interface ISocialChallengeReadRepository
{
    Task<SocialChallengeDto?> GetByIdAsync(Guid id);
    Task<PageResult<SocialChallengeDto>> GetAllAsync(int page, int pageSize);
}
