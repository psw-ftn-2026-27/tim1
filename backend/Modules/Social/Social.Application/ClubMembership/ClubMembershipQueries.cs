using Shared.Domain.Exceptions;
using Social.Application.Clubs;

namespace Social.Application.ClubMembership;

public sealed class ClubMembershipQueries
{
    private readonly IClubReadRepository _clubReadRepository;

    public ClubMembershipQueries(IClubReadRepository clubReadRepository)
    {
        _clubReadRepository = clubReadRepository;
    }

    public async Task<List<ClubMemberDto>> GetMembersAsync(Guid clubId)
    {
        var members = await _clubReadRepository.GetMembersAsync(clubId);
        if (members is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }

        return members;
    }

    public async Task<MyMembershipDto> GetMyMembershipAsync(Guid clubId, Guid userId)
    {
        var membership = await _clubReadRepository.GetMembershipAsync(clubId, userId);
        if (membership is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }

        return membership;
    }

    // P5: like GetByOwnerAsync, only the club's owner or a moderator gets its pending requests;
    // to anyone else the club does not exist.
    public async Task<List<MembershipRequestDto>> GetPendingRequestsAsync(Guid clubId, Guid userId)
    {
        var requests = await _clubReadRepository.GetPendingRequestsAsync(clubId, userId);
        if (requests is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }

        return requests;
    }
}
