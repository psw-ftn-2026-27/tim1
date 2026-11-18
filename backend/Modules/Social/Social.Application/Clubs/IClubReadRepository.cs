using Shared.Domain;
using Social.Application.ClubInviting;
using Social.Application.ClubMembership;

namespace Social.Application.Clubs;

public interface IClubReadRepository
{
    Task<ClubDto?> GetByIdAsync(Guid id);
    Task<List<ClubDto>> GetByOwnerAsync(Guid ownerId);
    Task<PageResult<ClubDto>> GetAllAsync(int page, int pageSize);
    Task<List<ClubMemberDto>?> GetMembersAsync(Guid clubId);
    Task<MyMembershipDto?> GetMembershipAsync(Guid clubId, Guid userId);
    Task<List<MembershipRequestDto>?> GetPendingRequestsAsync(Guid clubId, Guid resolverId);
    Task<List<SentInvitationDto>?> GetSentInvitationsAsync(Guid clubId, Guid ownerId);
    Task<List<MyInvitationDto>> GetInvitationsOfUserAsync(Guid userId);
}
